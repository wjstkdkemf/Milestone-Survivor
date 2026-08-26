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
    private readonly HashSet<EnemyProjectile> registeredProjectiles = new HashSet<EnemyProjectile>();

    private NativeArray<float2> projectilePositions;
    public float playerHitRadius = 0.3f; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (projectilePositions.IsCreated) projectilePositions.Dispose();
    }

    public void RegisterProjectile(EnemyProjectile projectile)
    {
        if (projectile == null || registeredProjectiles.Contains(projectile))
            return;

        registeredProjectiles.Add(projectile);
        activeProjectiles.Add(projectile);
    }

    public void UnregisterProjectile(EnemyProjectile projectile)
    {
        if (projectile == null || !registeredProjectiles.Remove(projectile))
            return;

        int index = activeProjectiles.IndexOf(projectile);
        if (index >= 0)
        {
            int lastIndex = activeProjectiles.Count - 1;
            activeProjectiles[index] = activeProjectiles[lastIndex];
            activeProjectiles.RemoveAt(lastIndex);
        }
    }

    private void LateUpdate()
    {
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            if (activeProjectiles[i] == null || !activeProjectiles[i].gameObject.activeInHierarchy)
            {
                if (activeProjectiles[i] != null)
                    registeredProjectiles.Remove(activeProjectiles[i]);

                activeProjectiles[i] = activeProjectiles[activeProjectiles.Count - 1];
                activeProjectiles.RemoveAt(activeProjectiles.Count - 1);
            }
        }

        int count = activeProjectiles.Count;
        if (count == 0 ||
            !TryGetPlayerReferences(out Transform playerCenter, out IDamageable playerDamageable) ||
            !TryGetReadyWorld(out float cellSize, out NativeParallelHashMap<Vector2Int, byte> globalWallMap))
        {
            return;
        }

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

        float3 playerPos3 = playerCenter.position;
        float2 playerPos = playerPos3.xy;

        NativeList<int> wallHitIndices = new NativeList<int>(count, Allocator.TempJob);
        NativeList<int> playerHitIndices = new NativeList<int>(count, Allocator.TempJob);

        ProjectileCollisionJob job = new ProjectileCollisionJob
        {
            positions = projectilePositions,
            playerPos = playerPos,
            playerHitRadiusSqr = playerHitRadius * playerHitRadius,
            
            cellSize = cellSize,
            globalWallMap = globalWallMap,

            wallHitResults = wallHitIndices.AsParallelWriter(),
            playerHitResults = playerHitIndices.AsParallelWriter()
        };

        JobHandle handle = job.Schedule(count, 64);
        handle.Complete();

        ProcessCollisionResults(wallHitIndices, playerHitIndices, playerDamageable);

        // 임시 리스트 메모리 해제
        wallHitIndices.Dispose();
        playerHitIndices.Dispose();
    }

    private void ProcessCollisionResults(
        NativeList<int> wallHitIndices,
        NativeList<int> playerHitIndices,
        IDamageable playerDamageable)
    {
        Dictionary<int, bool> hitMap = new Dictionary<int, bool>();

        for (int i = 0; i < wallHitIndices.Length; i++)
        {
            int index = wallHitIndices[i];
            if (!hitMap.ContainsKey(index))
                hitMap.Add(index, false);
        }

        for (int i = 0; i < playerHitIndices.Length; i++)
        {
            int index = playerHitIndices[i];
            hitMap[index] = true;
        }

        List<int> sortedIndices = new List<int>(hitMap.Keys);
        sortedIndices.Sort((a, b) => b.CompareTo(a));

        for (int i = 0; i < sortedIndices.Count; i++)
        {
            int index = sortedIndices[i];
            if (index < 0 || index >= activeProjectiles.Count)
                continue;

            EnemyProjectile projectile = activeProjectiles[index];
            if (projectile == null || !projectile.gameObject.activeInHierarchy)
                continue;

            if (hitMap[index])
                playerDamageable.TakeDamage(projectile.damage);

            projectile.SelfDestroy();
        }
    }

    private bool TryGetPlayerReferences(out Transform playerCenter, out IDamageable playerDamageable)
    {
        playerCenter = null;
        playerDamageable = null;

        if (GameManager.Instance == null || GameManager.Instance.Player == null)
            return false;

        GameObject playerObject = GameManager.Instance.Player;
        playerCenter = playerObject.transform.Find("CenterPosition");
        if (playerCenter == null)
            playerCenter = playerObject.transform;

        playerDamageable = playerObject.GetComponent<IDamageable>();
        return playerDamageable != null;
    }

    private bool TryGetReadyWorld(out float cellSize, out NativeParallelHashMap<Vector2Int, byte> globalWallMap)
    {
        cellSize = 0f;
        globalWallMap = default;

        if (EnemySwarmSystem.Instance == null ||
            EnemySwarmSystem.Instance.cellSize <= 0f ||
            InfiniteTilemapManager.Instance == null ||
            !InfiniteTilemapManager.Instance.globalWallMap.IsCreated)
        {
            return false;
        }

        cellSize = EnemySwarmSystem.Instance.cellSize;
        globalWallMap = InfiniteTilemapManager.Instance.globalWallMap;
        return true;
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
