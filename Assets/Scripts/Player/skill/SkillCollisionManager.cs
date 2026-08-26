using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SkillCollisionManager : MonoBehaviour
{
    public static SkillCollisionManager Instance;

    public List<SkillProjectileBase> activeSkills = new List<SkillProjectileBase>(400);
    private readonly HashSet<SkillProjectileBase> registeredSkills = new HashSet<SkillProjectileBase>();

    private NativeArray<float2> skillPositions;
    private NativeArray<float> skillRadii;
    private NativeArray<int> skillMaxHits;
    
    private NativeParallelMultiHashMap<int, int> hitResults;

    private void Awake() 
    { 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this; 
        hitResults = new NativeParallelMultiHashMap<int, int>(3000, Allocator.Persistent);
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (skillPositions.IsCreated) skillPositions.Dispose();
        if (skillRadii.IsCreated) skillRadii.Dispose();
        if (skillMaxHits.IsCreated) skillMaxHits.Dispose();
        if (hitResults.IsCreated) hitResults.Dispose();
    }

    public void RegisterSkill(SkillProjectileBase skill)
    {
        if (skill == null || registeredSkills.Contains(skill))
            return;

        registeredSkills.Add(skill);
        activeSkills.Add(skill);
    }

    public void UnregisterSkill(SkillProjectileBase skill)
    {
        if (skill == null || !registeredSkills.Remove(skill))
            return;

        int index = activeSkills.IndexOf(skill);
        if (index >= 0)
        {
            int lastIndex = activeSkills.Count - 1;
            activeSkills[index] = activeSkills[lastIndex];
            activeSkills.RemoveAt(lastIndex);
        }
    }

    private void LateUpdate()
    {
        for (int i = activeSkills.Count - 1; i >= 0; i--)
        {
            if (activeSkills[i] == null || !activeSkills[i].gameObject.activeInHierarchy)
            {
                if (activeSkills[i] != null)
                    registeredSkills.Remove(activeSkills[i]);

                activeSkills[i] = activeSkills[activeSkills.Count - 1];
                activeSkills.RemoveAt(activeSkills.Count - 1);
            }
        }

        int skillCount = activeSkills.Count;
        if (skillCount == 0 || !TryGetReadySwarm(out EnemySwarmSystem swarm)) return;

        if (!skillPositions.IsCreated || skillPositions.Length < skillCount)
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
        int requiredHitCapacity = Mathf.Max(skillCount * 10, swarm.activeEnemies.Count);
        if (hitResults.Capacity < requiredHitCapacity) // 적절한 용량 유지
        {
            hitResults.Dispose();
            hitResults = new NativeParallelMultiHashMap<int, int>(Mathf.Max(1, requiredHitCapacity * 2), Allocator.Persistent);
        }

        SkillCollisionJob colJob = new SkillCollisionJob
        {
            skillPositions = skillPositions,
            skillRadii = skillRadii,
            skillMaxHits = skillMaxHits,
            
            enemyPositions = swarm.positions,
            enemyGrid = swarm.grid,
            enemyRadius = swarm.enemyRadius,
            cellSize = swarm.cellSize,

            nextHitTimes = swarm.nextHitTimes,
            currentTime = Time.time,
            enemyRadii = swarm.enemyRadii,
            maxEnemyRadius = swarm.MaxEnemyRadius,
            
            hitResults = hitResults.AsParallelWriter() 
        };

        JobHandle handle = colJob.Schedule(skillCount, 64);
        handle.Complete();

        for (int i = 0; i < skillCount; i++)
        {
             if (hitResults.TryGetFirstValue(i, out int enemyIndex, out var it))
            {
                var skill = activeSkills[i];
                if (skill == null || !skill.gameObject.activeInHierarchy) continue;
                 do
                 {
                     Enemy hitEnemy = swarm.GetEnemyByIndex(enemyIndex);
                     if (hitEnemy != null && hitEnemy.currentNormalState != Enemy.EnemyState.Dead)
                     {
                        float distSqr = (hitEnemy.transform.position - skill.transform.position).sqrMagnitude;
                        float allowedDist = skill.hitRadius + hitEnemy.CollisionRadius;

                        if (distSqr <= (allowedDist * allowedDist) + 0.1f)
                        {
                            hitEnemy.TakeDamage(skill.damage);
                            skill.OnHit(hitEnemy);
                            if (enemyIndex >= 0 && enemyIndex < swarm.nextHitTimes.Length)
                                swarm.nextHitTimes[enemyIndex] = Time.time + 0.1f;

                            if (!skill.gameObject.activeInHierarchy) 
                            {
                                break; 
                            }
                        }
                     }
                 } while (hitResults.TryGetNextValue(out enemyIndex, ref it));
             }
         }
    }

    private bool TryGetReadySwarm(out EnemySwarmSystem swarm)
    {
        swarm = EnemySwarmSystem.Instance;

        if (swarm == null ||
            !swarm.positions.IsCreated ||
            !swarm.grid.IsCreated ||
            !swarm.nextHitTimes.IsCreated ||
            !swarm.enemyRadii.IsCreated ||
            swarm.positions.Length == 0 ||
            swarm.enemyRadii.Length == 0 ||
            swarm.nextHitTimes.Length == 0 ||
            swarm.cellSize <= 0f)
        {
            return false;
        }

        return true;
    }

    [BurstCompile]
    public struct SkillCollisionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> skillPositions;
        [ReadOnly] public NativeArray<float> skillRadii;
        [ReadOnly] public NativeArray<int> skillMaxHits; // 💡 읽기 전용

        [ReadOnly] public NativeArray<float2> enemyPositions;
        [ReadOnly] public NativeParallelMultiHashMap<int, int> enemyGrid;
        [ReadOnly] public NativeArray<float> enemyRadii;
        public float maxEnemyRadius;

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
            //int cellRange = (int)math.ceil((sRad + maxEnemyRadius) / cellSize);

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
                            float combinedRadius = sRad + enemyRadii[enemyIndex];

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
