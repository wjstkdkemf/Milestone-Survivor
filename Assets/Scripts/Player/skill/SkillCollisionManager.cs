using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SkillCollisionManager : MonoBehaviour
{
    public static SkillCollisionManager Instance;

    public List<SkillProjectileBase> activeSkills = new List<SkillProjectileBase>(200);

    private NativeArray<float2> skillPositions;
    private NativeArray<float> skillRadii;
    private NativeArray<int> skillMaxHits; 
    
    private NativeParallelMultiHashMap<int, int> hitResults;

    private void Awake() 
    { 
        Instance = this; 
        hitResults = new NativeParallelMultiHashMap<int, int>(3000, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (skillPositions.IsCreated) skillPositions.Dispose();
        if (skillRadii.IsCreated) skillRadii.Dispose();
        if (skillMaxHits.IsCreated) skillMaxHits.Dispose();
        if (hitResults.IsCreated) hitResults.Dispose();
    }

    private void LateUpdate()
    {
        int skillCount = activeSkills.Count;
        if (skillCount == 0 || EnemySwarmSystem.Instance.positions.Length == 0) return;

        if (!skillPositions.IsCreated || skillPositions.Length != skillCount)
        {
            if (skillPositions.IsCreated) skillPositions.Dispose();
            if (skillRadii.IsCreated) skillRadii.Dispose();
            if (skillMaxHits.IsCreated) skillMaxHits.Dispose();

            int newSize = Mathf.NextPowerOfTwo(skillCount);
            skillPositions = new NativeArray<float2>(newSize, Allocator.Persistent);
            skillRadii = new NativeArray<float>(newSize, Allocator.Persistent);
            skillMaxHits = new NativeArray<int>(newSize, Allocator.Persistent);
        }

        for (int i = 0; i < skillCount; i++)
        {
            SkillProjectileBase s = activeSkills[i];
            skillPositions[i] = new float2(s.transform.position.x, s.transform.position.y);
            skillRadii[i] = s.hitRadius;
            skillMaxHits[i] = s.maxHits;
        }

        hitResults.Clear();
        if (hitResults.Capacity < skillCount * 5) // 적절한 용량 유지
        {
            hitResults.Dispose();
            hitResults = new NativeParallelMultiHashMap<int, int>(skillCount * 10, Allocator.Persistent);
        }

        SkillCollisionJob colJob = new SkillCollisionJob
        {
            skillPositions = skillPositions,
            skillRadii = skillRadii,
            skillMaxHits = skillMaxHits,
            
            enemyPositions = EnemySwarmSystem.Instance.positions,
            enemyGrid = EnemySwarmSystem.Instance.grid,
            enemyRadius = EnemySwarmSystem.Instance.enemyRadius,
            cellSize = EnemySwarmSystem.Instance.cellSize,

            nextHitTimes = EnemySwarmSystem.Instance.nextHitTimes,
            currentTime = Time.time,
            
            hitResults = hitResults.AsParallelWriter() 
        };

        JobHandle handle = colJob.Schedule(skillCount, 64);
        handle.Complete();

        for (int i = 0; i < skillCount; i++)
        {
             if (hitResults.TryGetFirstValue(i, out int enemyIndex, out var it))
            {
                var skill = activeSkills[i];
                 do
                 {
                     Enemy hitEnemy = EnemySwarmSystem.Instance.GetEnemyByIndex(enemyIndex);
                     if (hitEnemy != null && hitEnemy.currentNormalState != Enemy.EnemyState.Dead)
                     {
                         hitEnemy.TakeDamage(skill.damage);
                         skill.OnHit(hitEnemy);
                         EnemySwarmSystem.Instance.nextHitTimes[enemyIndex] = Time.time + 0.1f;
                     }
                 } while (hitResults.TryGetNextValue(out enemyIndex, ref it));
             }
         }
    }

    [BurstCompile]
    public struct SkillCollisionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> skillPositions;
        [ReadOnly] public NativeArray<float> skillRadii;
        [ReadOnly] public NativeArray<int> skillMaxHits; // 💡 읽기 전용

        [ReadOnly] public NativeArray<float2> enemyPositions;
        [ReadOnly] public NativeParallelMultiHashMap<int, int> enemyGrid;

        public float enemyRadius;
        public float cellSize;
        [ReadOnly]public NativeArray<float> nextHitTimes; 
        public float currentTime; // 현재 시간

        public NativeParallelMultiHashMap<int, int>.ParallelWriter hitResults;

        public void Execute(int skillIndex)
        {
            float2 sPos = skillPositions[skillIndex];
            float sRad = skillRadii[skillIndex];
            int maxHits = skillMaxHits[skillIndex];
            
            int currentHits = 0; // 💡 이 프레임에서 몇 대 때렸는지 카운트

            int2 centerCell = new int2(
                (int)math.floor(sPos.x / cellSize),
                (int)math.floor(sPos.y / cellSize)
            );

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    int2 neighbor = centerCell + new int2(x, y);
                    int hash = neighbor.x * 73856093 ^ neighbor.y * 19349663;

                    if (enemyGrid.TryGetFirstValue(hash, out int enemyIndex, out var it))
                    {
                        do
                        {
                            if (currentTime < nextHitTimes[enemyIndex]) continue;

                            float2 diff = sPos - enemyPositions[enemyIndex];
                            float sqrDist = math.lengthsq(diff);
                            float combinedRadius = sRad + enemyRadius;

                            if (sqrDist < combinedRadius * combinedRadius)
                            {
                                hitResults.Add(skillIndex, enemyIndex);
                                currentHits++;

                                if (maxHits > 0 && currentHits >= maxHits)
                                {
                                    return;
                                }
                            }
                        } while (enemyGrid.TryGetNextValue(out enemyIndex, ref it));
                    }
                }
            }
        }
    }
}