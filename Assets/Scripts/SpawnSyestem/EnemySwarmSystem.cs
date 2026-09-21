using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;
using System.Linq;
public class EnemySwarmSystem : MonoBehaviour
{
    public static EnemySwarmSystem Instance;

    public List<Enemy> activeEnemies = new List<Enemy>(3000);
    private readonly HashSet<Enemy> registeredEnemies = new HashSet<Enemy>();
    private TransformAccessArray transformAccessArray;

    // Job에 넘겨줄 데이터 배열들
    public NativeArray<float2> positions;
    private NativeArray<float> speeds;
    private NativeArray<bool> canMove; // Chasing 상태인지 여부
    public NativeArray<float> enemyRadii;
    public float MaxEnemyRadius { get; private set; }
    public NativeArray<float> nextHitTimes;
    public NativeParallelMultiHashMap<long, int> grid;

    [Header("Swarm Settings")]
    public float enemyRadius = 0.5f; // 몬스터의 크기
    public float cellSize;
    [SerializeField, Min(1)] private int initialCapacity = 3000;
    private Transform playerCenter;

    private readonly HashSet<Enemy> addReservations = new HashSet<Enemy>();
    private readonly HashSet<Enemy> removeReservations = new HashSet<Enemy>();
    private uint[] indexedSpawnVersions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // 최대 몬스터 수용량 미리 할당
        transformAccessArray = new TransformAccessArray(initialCapacity);
        grid = new NativeParallelMultiHashMap<long, int>(initialCapacity, Allocator.Persistent);
        nextHitTimes = new NativeArray<float>(initialCapacity, Allocator.Persistent);
        indexedSpawnVersions = new uint[initialCapacity];

        enemyRadius = Mathf.Max(0.01f, enemyRadius);
        cellSize = Mathf.Max(0.01f, enemyRadius * 2);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (transformAccessArray.isCreated) transformAccessArray.Dispose();
        DisposeMovementBuffers();
        if (grid.IsCreated) grid.Dispose();
        if (nextHitTimes.IsCreated) nextHitTimes.Dispose();
    }
    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        if(removeReservations.Contains(enemy))
        {
            removeReservations.Remove(enemy);
            addReservations.Add(enemy);
            return;
        }

        if (!registeredEnemies.Contains(enemy))
            addReservations.Add(enemy);
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        addReservations.Remove(enemy);

        if (registeredEnemies.Contains(enemy))
            removeReservations.Add(enemy);
    }

    private void RegisterEnemyImmediately(Enemy enemy)
    {
        if (enemy == null)
            return;
        
        if(registeredEnemies.Contains(enemy))
        {   
            ReSpawnVersion(enemy);
            return;
        }

        EnsureNextHitCapacity(activeEnemies.Count + 1);

        registeredEnemies.Add(enemy);
        activeEnemies.Add(enemy);
        transformAccessArray.Add(enemy.transform);

        int newIndex = activeEnemies.Count - 1;

        if (nextHitTimes.IsCreated)
        {
            nextHitTimes[newIndex] = 0f;
        }
        indexedSpawnVersions[newIndex] = enemy.SpawnVersion;
    }

    private void UnregisterEnemyImmediately(Enemy enemy)
    {
        if (ReferenceEquals(enemy,null))
            return;
        if(!registeredEnemies.Remove(enemy))
            return;

        int index = activeEnemies.IndexOf(enemy);
        if (index >= 0)
        {
            int lastIndex = activeEnemies.Count - 1;
            
            activeEnemies[index] = activeEnemies[lastIndex];
            activeEnemies.RemoveAt(lastIndex);

            transformAccessArray.RemoveAtSwapBack(index);

            if (nextHitTimes.IsCreated)
            {
                nextHitTimes[index] = nextHitTimes[lastIndex];
                nextHitTimes[lastIndex] = 0f;
            }

            indexedSpawnVersions[index] = indexedSpawnVersions[lastIndex];
            indexedSpawnVersions[lastIndex] = 0;
        }
    }

    private void Update()
    {
        ApplyReservations();
        MaxEnemyRadius = 0f;
        grid.Clear();

        int count = activeEnemies.Count;
        if (count == 0 || !TryGetPlayerCenter(out Transform targetCenter)) return;
        if (InfiniteTilemapManager.Instance == null ||
            !InfiniteTilemapManager.Instance.globalWallMap.IsCreated)
        {
            return;
        }

        // 배열 크기 맞추기 및 데이터 갱신
        EnsureCapacity(count);

        for (int i = 0; i < count; i++)
        {
            Enemy e = activeEnemies[i];
            if (e == null)
            {
                canMove[i] = false;
                continue;
            }

            float3 pos = e.transform.position;
            positions[i] = pos.xy;
            
            // 넉백 중이거나 공격 중이면 움직이지 않도록 설정
            bool isChasing = e.currentNormalState == Enemy.EnemyState.Chasing && 
                                                    !e.stopMoving && 
                                                    e.GetKnockBackTime() <= 0 &&
                                                    e.useSwarmMovement &&
                                                    !e.casting;
            canMove[i] = isChasing;
            speeds[i] = e.GetSpeed(); // Enemy의 현재 속도
            float radius = e.CollisionRadius;
            enemyRadii[i] = radius;
            MaxEnemyRadius = Mathf.Max(MaxEnemyRadius, radius);
        }

        grid.Clear();//안전 보호용 한번더 클리어
        
        if (grid.Capacity < activeEnemies.Count)
        {
            grid.Dispose();
            grid = new NativeParallelMultiHashMap<long, int>(activeEnemies.Count * 2, Allocator.Persistent); 
        }

        for (int i = 0; i < count; i++)
        {
            int2 cell = GetCell(positions[i]);
            long hash = Hash(cell);
            grid.Add(hash, i);
        }
        float3 PlayerPos = targetCenter.position;

        SwarmMoveJob moveJob = new SwarmMoveJob
        {
            positions = positions,
            speeds = speeds,
            canMove = canMove,
            grid = grid,

            targetPos = PlayerPos.xy,
            deltaTime = Time.deltaTime,

            enemyRadius = enemyRadius,
            enemyRadiusSqr = enemyRadius * enemyRadius,
            cellSize = cellSize,

            enemyRadii = enemyRadii,
            maxEnemyRadius = MaxEnemyRadius,

            globalWallMap = InfiniteTilemapManager.Instance.globalWallMap
        };

        JobHandle handle = moveJob.Schedule(transformAccessArray);
        handle.Complete();

        //grid.Dispose();
    }
    private void ReSpawnVersion(Enemy enemy)
    {
        int count = activeEnemies.Count;
        for (int i = 0; i < count; i++)
        {
            if(activeEnemies[i] == enemy)
            {
                if (nextHitTimes.IsCreated)
                {
                    nextHitTimes[i] = 0f;
                }
                indexedSpawnVersions[i] = enemy.SpawnVersion;
            }
        }
    }
    private void EnsureCapacity(int requiredCount)
    {
        bool allCreated =   positions.IsCreated &&
                            speeds.IsCreated &&
                            canMove.IsCreated &&
                            enemyRadii.IsCreated;

        if (allCreated && positions.Length >= requiredCount)
            return;

        int nextCapacity = Mathf.Max(
            initialCapacity,
            Mathf.NextPowerOfTwo(Mathf.Max(1, requiredCount))
        );

        DisposeMovementBuffers();

        positions = new NativeArray<float2>(nextCapacity, Allocator.Persistent);

        speeds = new NativeArray<float>(nextCapacity, Allocator.Persistent);

        canMove = new NativeArray<bool>(nextCapacity, Allocator.Persistent);

        enemyRadii = new NativeArray<float>(nextCapacity, Allocator.Persistent);

        if(indexedSpawnVersions.Length < nextCapacity)
            Array.Resize(ref indexedSpawnVersions, nextCapacity);
    }
    private void EnsureNextHitCapacity(int requiredCapacity)
    {
        if (nextHitTimes.IsCreated && nextHitTimes.Length >= requiredCapacity)
        {
            return;
        }

        int newCapacity = Mathf.Max(requiredCapacity, nextHitTimes.IsCreated ? nextHitTimes.Length * 2 : initialCapacity);
        NativeArray<float> newNextHitTimes = new NativeArray<float>(newCapacity, Allocator.Persistent);

        if (nextHitTimes.IsCreated)
        {
            NativeArray<float>.Copy(nextHitTimes, newNextHitTimes, nextHitTimes.Length);
            nextHitTimes.Dispose();
        }

        if(indexedSpawnVersions.Length <= newCapacity)
            Array.Resize(ref indexedSpawnVersions,newCapacity);

        nextHitTimes = newNextHitTimes;
    }
    private void ApplyReservations()
    {
        foreach (Enemy enemy in removeReservations)
            UnregisterEnemyImmediately(enemy);

        removeReservations.Clear();

        foreach (Enemy enemy in addReservations)
            RegisterEnemyImmediately(enemy);

        addReservations.Clear();
    }
    private bool TryGetPlayerCenter(out Transform center)
    {
        if (playerCenter != null)
        {
            center = playerCenter;
            return true;
        }

        center = null;

        if (GameManager.Instance == null || GameManager.Instance.Player == null)
        {
            return false;
        }

        playerCenter = GameManager.Instance.Player.transform.Find("CenterPosition");
        if (playerCenter == null)
        {
            Debug.LogWarning("[EnemySwarmSystem] Player CenterPosition transform is missing.");
            return false;
        }

        center = playerCenter;
        return true;
    }
    public Enemy TryGetTarget(int Index)
    {
        if (Index < 0 || Index >= activeEnemies.Count)
            return null;

        Enemy enemy = activeEnemies[Index];

        if (enemy == null ||
            !enemy.isActiveAndEnabled ||
            enemy.currentNormalState == Enemy.EnemyState.Dead ||
            removeReservations.Contains(enemy) ||
            enemy.SpawnVersion != indexedSpawnVersions[Index])
        {
            return null;
        }

        return enemy;
    }
    public Enemy GetEnemyByIndex(int index)
    {
        return TryGetTarget(index);
    }
    public int2 GetCell(float2 pos)
    {
        float safeCellSize = Mathf.Max(0.01f, cellSize);
        return new int2(
            (int)math.floor(pos.x / safeCellSize),
            (int)math.floor(pos.y / safeCellSize)
        );
    }
    public static long Hash(int2 cell)
    {
        return unchecked(((long)cell.x << 32) | (uint)cell.y);
    }
    public Enemy GetClosestEnemy(Vector2 center, float range, HashSet<Enemy> excludedEnemies = null)
    {
        if (!grid.IsCreated || activeEnemies.Count == 0 || cellSize <= 0f)
        {
            return null;
        }

        float closestDistanceSqr = range * range;
        Enemy closestEnemy = null;

        int minCellX = Mathf.FloorToInt((center.x - range) / cellSize);
        int maxCellX = Mathf.FloorToInt((center.x + range) / cellSize);
        int minCellY = Mathf.FloorToInt((center.y - range) / cellSize);
        int maxCellY = Mathf.FloorToInt((center.y + range) / cellSize);

        for (int x = minCellX; x <= maxCellX; x++)
        {
            for (int y = minCellY; y <= maxCellY; y++)
            {
                long hash = unchecked(((long)x << 32) | (uint)y);
                NativeParallelMultiHashMapIterator<long> it;
                
                if (grid.TryGetFirstValue(hash, out int enemyIndex, out it))
                {
                    do
                    {
                        if (enemyIndex < 0 || enemyIndex >= activeEnemies.Count) continue;

                        Enemy enemy = TryGetTarget(enemyIndex);

                        if (enemy == null || enemy.currentNormalState == Enemy.EnemyState.Dead) continue;
                        if (excludedEnemies != null && excludedEnemies.Contains(enemy)) continue;

                        float distSqr = (enemy.transform.position - (Vector3)center).sqrMagnitude;
                        if (distSqr < closestDistanceSqr)
                        {
                            closestDistanceSqr = distSqr;
                            closestEnemy = enemy;
                        }

                    } while (grid.TryGetNextValue(out enemyIndex, ref it));
                }
            }
        }
        return closestEnemy;
    }
    public void GetEnemiesInRadius(Vector2 center, float range, List<int> results)
    {
        if (results == null)
        {
            return;
        }

        results.Clear(); 
        if (!grid.IsCreated || activeEnemies.Count == 0 || cellSize <= 0f)
        {
            return;
        }

        float rangeSqr = range * range;

        int minCellX = Mathf.FloorToInt((center.x - range) / cellSize);
        int maxCellX = Mathf.FloorToInt((center.x + range) / cellSize);
        int minCellY = Mathf.FloorToInt((center.y - range) / cellSize);
        int maxCellY = Mathf.FloorToInt((center.y + range) / cellSize);

        for (int x = minCellX; x <= maxCellX; x++)
        {
            for (int y = minCellY; y <= maxCellY; y++)
            {
                long hash = unchecked(((long)x << 32) | (uint)y);
                NativeParallelMultiHashMapIterator<long> it;
                
                if (grid.TryGetFirstValue(hash, out int enemyIndex, out it))
                {
                    do
                    {
                        if (enemyIndex < 0 || enemyIndex >= activeEnemies.Count) continue;

                        Enemy enemy = activeEnemies[enemyIndex];

                        if (enemy == null || enemy.currentNormalState == Enemy.EnemyState.Dead) continue;

                        float distSqr = (enemy.transform.position - (Vector3)center).sqrMagnitude;
                        if (distSqr <= rangeSqr)
                        {
                            results.Add(enemyIndex);
                        }

                    } while (grid.TryGetNextValue(out enemyIndex, ref it));
                }
            }
        }
    }
    private void DisposeMovementBuffers()
    {
        if (positions.IsCreated) positions.Dispose();
        if (speeds.IsCreated) speeds.Dispose();
        if (canMove.IsCreated) canMove.Dispose();
        if (enemyRadii.IsCreated) enemyRadii.Dispose();
    }

    [BurstCompile]
    private struct SwarmMoveJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<float2> positions;
        [ReadOnly] public NativeArray<float> speeds;
        [ReadOnly] public NativeArray<bool> canMove;

        [ReadOnly] public NativeParallelMultiHashMap<long, int> grid;
        [ReadOnly] public NativeParallelHashMap<Vector2Int, byte> globalWallMap;
        [ReadOnly] public NativeArray<float> enemyRadii;
        public float2 targetPos;
        public float deltaTime;
        public float enemyRadius; 
        public float enemyRadiusSqr;
        public float cellSize;
        public float maxEnemyRadius;


        public void Execute(int index, TransformAccess transform)
        {
            if (!canMove[index]) return;

            float3 pos3 = transform.position;
            float2 myPos = pos3.xy;
            float2 collisionPush = float2.zero;

            int2 myCell = GetCell(myPos);

            float myRadius = enemyRadii[index];

            int cellRange = (int)math.ceil((myRadius + maxEnemyRadius) / cellSize);

            for (int y = -cellRange; y <= cellRange; y++)
            {
                for (int x = -cellRange; x <= cellRange; x++)
                {
                    int2 neighbor = myCell + new int2(x, y);
                    long hash = Hash(neighbor);

                    NativeParallelMultiHashMapIterator<long> it;
                    int otherIndex;

                    if (grid.TryGetFirstValue(hash, out otherIndex, out it))
                    {
                        do
                        {
                            if (otherIndex == index) continue;

                            float2 diff = myPos - positions[otherIndex];
                            float sqrDist = math.lengthsq(diff);

                            float combinedRadius = myRadius + enemyRadii[otherIndex];

                            if (sqrDist < (enemyRadiusSqr * 4f) && sqrDist > 0.0001f)
                            {
                                float dist = math.sqrt(sqrDist);
                                float penetration = combinedRadius - dist;

                                collisionPush += (diff / dist) * penetration * 5f;
                            }

                        } while (grid.TryGetNextValue(out otherIndex, ref it));
                    }
                }
            }

            float2 toTarget = targetPos - myPos;
            float distSq = math.lengthsq(toTarget);
            float2 targetDir = distSq > 0.0001f ? toTarget * math.rsqrt(distSq) : float2.zero;

            float2 velocity = targetDir * speeds[index];

            float2 move = (velocity + collisionPush) * deltaTime;

            // 벽 체크
            if (IsWall(myPos + new float2(move.x, 0))) move.x = 0;
            if (IsWall(myPos + new float2(0, move.y))) move.y = 0;
            pos3.xy = myPos + move;
            transform.position = pos3;
        }
        private static long Hash(int2 cell)
        {
            return unchecked(((long)cell.x << 32) | (uint)cell.y);
        }
        private int2 GetCell(float2 pos)
        {
            return new int2(
                (int)math.floor(pos.x / cellSize),
                (int)math.floor(pos.y / cellSize)
            );
        }
        private bool IsWall(float2 pos)
        {
            int2 cell = new int2(
                (int)math.floor(pos.x / cellSize),
                (int)math.floor(pos.y / cellSize)
            );

            return globalWallMap.ContainsKey(new Vector2Int(cell.x, cell.y));
        }
    }
}
