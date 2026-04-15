using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class EnemyProjectileManager : MonoBehaviour
{
    public static EnemyProjectileManager Instance;

    public List<EnemyProjectile> activeProjectiles = new List<EnemyProjectile>(1000);

    private NativeArray<float2> projectilePositions;
    public float playerHitRadius = 0.3f; 

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (projectilePositions.IsCreated) projectilePositions.Dispose();
    }

    public void RegisterProjectile(EnemyProjectile projectile)
    {
        activeProjectiles.Add(projectile);
    }

    private void LateUpdate()
    {
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            if (activeProjectiles[i] == null || !activeProjectiles[i].gameObject.activeInHierarchy)
            {
                activeProjectiles[i] = activeProjectiles[activeProjectiles.Count - 1];
                activeProjectiles.RemoveAt(activeProjectiles.Count - 1);
            }
        }

        int count = activeProjectiles.Count;
        if (count == 0 || GameManager.Instance.Player == null) return;

        if (!projectilePositions.IsCreated || projectilePositions.Length < count)
        {
            if (projectilePositions.IsCreated) projectilePositions.Dispose();
            int newSize = Mathf.NextPowerOfTwo(count);
            projectilePositions = new NativeArray<float2>(newSize, Allocator.Persistent);
        }

        for (int i = 0; i < count; i++)
        {
            projectilePositions[i] = new float2(activeProjectiles[i].transform.position.x, activeProjectiles[i].transform.position.y);
        }

        float3 playerPos3 = GameManager.Instance.Player.transform.Find("CenterPosition").position;
        float2 playerPos = playerPos3.xy;

        NativeList<int> wallHitIndices = new NativeList<int>(count, Allocator.TempJob);
        NativeList<int> playerHitIndices = new NativeList<int>(count, Allocator.TempJob);

        ProjectileCollisionJob job = new ProjectileCollisionJob
        {
            positions = projectilePositions,
            playerPos = playerPos,
            playerHitRadiusSqr = playerHitRadius * playerHitRadius,
            
            cellSize = EnemySwarmSystem.Instance.cellSize,
            globalWallMap = InfiniteTilemapManager.Instance.globalWallMap,

            wallHitResults = wallHitIndices.AsParallelWriter(),
            playerHitResults = playerHitIndices.AsParallelWriter()
        };

        JobHandle handle = job.Schedule(count, 64);
        handle.Complete();

        for (int i = 0; i < wallHitIndices.Length; i++)
        {
            int idx = wallHitIndices[i];
            activeProjectiles[idx].SelfDestroy();
        }

        for (int i = 0; i < playerHitIndices.Length; i++)
        {
            int idx = playerHitIndices[i];
            EnemyProjectile proj = activeProjectiles[idx];

            if (proj.gameObject.activeInHierarchy)
            {
                IDamageable playerHealth = GameManager.Instance.Player.GetComponent<IDamageable>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(proj.damage);
                }
                
                proj.SelfDestroy();
            }
        }

        // 임시 리스트 메모리 해제
        wallHitIndices.Dispose();
        playerHitIndices.Dispose();
    }

    [BurstCompile]
    private struct ProjectileCollisionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> positions;
        [ReadOnly] public NativeParallelHashMap<Vector2Int, byte> globalWallMap;

        public float2 playerPos;
        public float playerHitRadiusSqr;
        public float cellSize;

        public NativeList<int>.ParallelWriter wallHitResults;
        public NativeList<int>.ParallelWriter playerHitResults;

        public void Execute(int index)
        {
            float2 pos = positions[index];

            int2 cell = new int2(
                (int)math.floor(pos.x / cellSize),
                (int)math.floor(pos.y / cellSize)
            );

            if (globalWallMap.ContainsKey(new Vector2Int(cell.x, cell.y)))
            {
                wallHitResults.AddNoResize(index);
                return; 
            }

            float2 diff = pos - playerPos;
            float distSqr = math.lengthsq(diff);

            if (distSqr <= playerHitRadiusSqr)
            {
                playerHitResults.AddNoResize(index);
            }
        }
    }
}