using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;
public class EnemySwarmSystem : MonoBehaviour
{
    public static EnemySwarmSystem Instance;

    public List<Enemy> activeEnemies = new List<Enemy>(3000);
    private TransformAccessArray transformAccessArray;

    // Job에 넘겨줄 데이터 배열들
    public NativeArray<float2> positions;
    private NativeArray<float> speeds;
    private NativeArray<bool> canMove; // Chasing 상태인지 여부
    public NativeArray<float> nextHitTimes;
    public NativeParallelMultiHashMap<int, int> grid;

    [Header("Swarm Settings")]
    public float enemyRadius = 0.5f; // 몬스터의 크기
    public float cellSize;

    private void Awake()
    {
        Instance = this;
        // 최대 몬스터 수용량 미리 할당
        transformAccessArray = new TransformAccessArray(3000);
        grid = new NativeParallelMultiHashMap<int, int>(3000, Allocator.Persistent);
        nextHitTimes = new NativeArray<float>(3000, Allocator.Persistent);

        cellSize = enemyRadius * 2;
    }

    private void OnDestroy()
    {
        if (transformAccessArray.isCreated) transformAccessArray.Dispose();
        if (positions.IsCreated) positions.Dispose();
        if (speeds.IsCreated) speeds.Dispose();
        if (canMove.IsCreated) canMove.Dispose();
        if (grid.IsCreated) grid.Dispose();
        if (nextHitTimes.IsCreated) nextHitTimes.Dispose();
    }

    public void RegisterEnemy(Enemy enemy)
    {
        activeEnemies.Add(enemy);
        transformAccessArray.Add(enemy.transform);

        int newIndex = activeEnemies.Count - 1;

        if (nextHitTimes.IsCreated)
        {
            nextHitTimes[newIndex] = 0f;
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
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
        }
    }

    private void Update()
    {
        int count = activeEnemies.Count;
        if (count == 0 || GameManager.Instance.Player == null) return;

        // 매 프레임 배열 크기 맞추기 및 데이터 갱신
        if (!positions.IsCreated || positions.Length != count)
        {
            if (positions.IsCreated) positions.Dispose();
            if (speeds.IsCreated) speeds.Dispose();
            if (canMove.IsCreated) canMove.Dispose();

            positions = new NativeArray<float2>(count, Allocator.Persistent);
            speeds = new NativeArray<float>(count, Allocator.Persistent);
            canMove = new NativeArray<bool>(count, Allocator.Persistent);
        }

        for (int i = 0; i < count; i++)
        {
            Enemy e = activeEnemies[i];
            float3 pos = e.transform.position;
            positions[i] = pos.xy;
            
            // 넉백 중이거나 공격 중이면 움직이지 않도록 설정
            bool isChasing = e.currentNormalState == Enemy.EnemyState.Chasing && 
                                                    !e.stopMoving && 
                                                    e.GetKnockBackTime() <= 0 &&
                                                    e.useSwarmMovement;
            canMove[i] = isChasing;
            speeds[i] = e.GetSpeed(); // Enemy의 현재 속도
        }

        grid.Clear();
        
        if (grid.Capacity < activeEnemies.Count)
        {
            grid.Dispose();
            grid = new NativeParallelMultiHashMap<int, int>(activeEnemies.Count * 2, Allocator.Persistent); 
        }

        for (int i = 0; i < count; i++)
        {
            int2 cell = GetCell(positions[i]);
            int hash = Hash(cell);
            grid.Add(hash, i);
        }
        float3 PlayerPos = GameManager.Instance.Player.transform.Find("CenterPosition").position;

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

            globalWallMap = InfiniteTilemapManager.Instance.globalWallMap
        };

        JobHandle handle = moveJob.Schedule(transformAccessArray);
        handle.Complete();

        //grid.Dispose();
    }
    public Enemy GetEnemyByIndex(int index)
    {
        if (index < 0 || index >= activeEnemies.Count)
        {
            return null;
        }

        return activeEnemies[index];
    }
    public int2 GetCell(float2 pos)
    {
        return new int2(
            (int)math.floor(pos.x / cellSize),
            (int)math.floor(pos.y / cellSize)
        );
    }
    int Hash(int2 cell)
    {
        return cell.x * 73856093 ^ cell.y * 19349663;
    }
    public Enemy GetClosestEnemy(Vector2 center, float range, HashSet<Enemy> excludedEnemies = null)
    {
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
                int hash = x * 73856093 ^ y * 19349663;
                NativeParallelMultiHashMapIterator<int> it;
                
                if (grid.TryGetFirstValue(hash, out int enemyIndex, out it))
                {
                    do
                    {
                        if (enemyIndex < 0 || enemyIndex >= activeEnemies.Count) continue;

                        Enemy enemy = activeEnemies[enemyIndex];

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
        results.Clear(); 
        float rangeSqr = range * range;

        int minCellX = Mathf.FloorToInt((center.x - range) / cellSize);
        int maxCellX = Mathf.FloorToInt((center.x + range) / cellSize);
        int minCellY = Mathf.FloorToInt((center.y - range) / cellSize);
        int maxCellY = Mathf.FloorToInt((center.y + range) / cellSize);

        for (int x = minCellX; x <= maxCellX; x++)
        {
            for (int y = minCellY; y <= maxCellY; y++)
            {
                int hash = x * 73856093 ^ y * 19349663;
                NativeParallelMultiHashMapIterator<int> it;
                
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

    [BurstCompile]
    private struct SwarmMoveJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<float2> positions;
        [ReadOnly] public NativeArray<float> speeds;
        [ReadOnly] public NativeArray<bool> canMove;

        [ReadOnly] public NativeParallelMultiHashMap<int, int> grid;
        [ReadOnly] public NativeParallelHashMap<Vector2Int, byte> globalWallMap;

        public float2 targetPos;
        public float deltaTime;
        public float enemyRadius; 
        public float enemyRadiusSqr;
        public float cellSize;

        public void Execute(int index, TransformAccess transform)
        {
            if (!canMove[index]) return;

            float3 pos3 = transform.position;
            float2 myPos = pos3.xy;
            float2 collisionPush = float2.zero;

            int2 myCell = GetCell(myPos);

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    int2 neighbor = myCell + new int2(x, y);
                    int hash = Hash(neighbor);

                    NativeParallelMultiHashMapIterator<int> it;
                    int otherIndex;

                    if (grid.TryGetFirstValue(hash, out otherIndex, out it))
                    {
                        do
                        {
                            if (otherIndex == index) continue;

                            float2 diff = myPos - positions[otherIndex];
                            float sqrDist = math.lengthsq(diff);

                            if (sqrDist < (enemyRadiusSqr * 4f) && sqrDist > 0.0001f)
                            {
                                float dist = math.sqrt(sqrDist);
                                float penetration = (enemyRadius * 2f) - dist;

                                collisionPush += (diff / dist) * penetration * 5f;
                            }

                        } while (grid.TryGetNextValue(out otherIndex, ref it));
                    }
                }
            }

            float2 targetDir = math.normalize(targetPos - myPos);
            float2 velocity = targetDir * speeds[index];

            float2 move = (velocity + collisionPush) * deltaTime;

            // 벽 체크
            if (IsWall(myPos + new float2(move.x, 0))) move.x = 0;
            if (IsWall(myPos + new float2(0, move.y))) move.y = 0;
            pos3.xy = myPos + move;
            transform.position = pos3;
        }
        private int Hash(int2 cell)
        {
            return cell.x * 73856093 ^ cell.y * 19349663;
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
