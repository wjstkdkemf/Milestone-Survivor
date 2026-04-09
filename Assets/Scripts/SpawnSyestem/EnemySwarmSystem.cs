using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class EnemySwarmSystem : MonoBehaviour
{
    public static EnemySwarmSystem Instance;

    private List<Enemy> activeEnemies = new List<Enemy>(3000);
    private TransformAccessArray transformAccessArray;

    // Job에 넘겨줄 데이터 배열들
    private NativeArray<Vector2> positions;
    private NativeArray<float> speeds;
    private NativeArray<bool> canMove; // Chasing 상태인지 여부

    [Header("Swarm Settings")]
    public float enemyRadius = 0.5f; // 몬스터의 크기

    private void Awake()
    {
        Instance = this;
        // 최대 몬스터 수용량 미리 할당
        transformAccessArray = new TransformAccessArray(3000);
    }

    private void OnDestroy()
    {
        if (transformAccessArray.isCreated) transformAccessArray.Dispose();
        if (positions.IsCreated) positions.Dispose();
        if (speeds.IsCreated) speeds.Dispose();
        if (canMove.IsCreated) canMove.Dispose();
    }

    public void RegisterEnemy(Enemy enemy)
    {
        activeEnemies.Add(enemy);
        transformAccessArray.Add(enemy.transform);
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        int index = activeEnemies.IndexOf(enemy);
        if (index >= 0)
        {
            activeEnemies.RemoveAt(index);
            transformAccessArray.RemoveAtSwapBack(index);
        }
    }

    private void Update()
    {
        int count = activeEnemies.Count;
        if (count == 0 || GameManager.Instance.Player == null) return;

        // 1. 매 프레임 배열 크기 맞추기 및 데이터 갱신
        if (!positions.IsCreated || positions.Length != count)
        {
            if (positions.IsCreated) positions.Dispose();
            if (speeds.IsCreated) speeds.Dispose();
            if (canMove.IsCreated) canMove.Dispose();

            positions = new NativeArray<Vector2>(count, Allocator.Persistent);
            speeds = new NativeArray<float>(count, Allocator.Persistent);
            canMove = new NativeArray<bool>(count, Allocator.Persistent);
        }

        for (int i = 0; i < count; i++)
        {
            Enemy e = activeEnemies[i];
            positions[i] = e.transform.position;
            
            // 넉백 중이거나 공격 중이면 움직이지 않도록 설정
            bool isChasing = e.currentNormalState == Enemy.EnemyState.Chasing && !e.stopMoving && e.GetKnockBackTime() <= 0;
            canMove[i] = isChasing;
            speeds[i] = e.GetSpeed(); // Enemy의 현재 속도
        }

        SwarmMoveJob moveJob = new SwarmMoveJob
        {
            positions = positions,
            speeds = speeds,
            canMove = canMove,
            targetPos = GameManager.Instance.Player.transform.Find("CenterPosition").position,
            deltaTime = Time.deltaTime,
            enemyRadius = enemyRadius,
            enemyRadiusSqr = enemyRadius * enemyRadius,
            globalWallMap = InfiniteTilemapManager.Instance.globalWallMap,
            cellSize = 1.0f
        };

        JobHandle handle = moveJob.Schedule(transformAccessArray);
        
        handle.Complete();
    }

    [BurstCompile]
    private struct SwarmMoveJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<Vector2> positions;
        [ReadOnly] public NativeArray<float> speeds;
        [ReadOnly] public NativeArray<bool> canMove;
        [ReadOnly] public NativeParallelHashMap<Vector2Int, byte> globalWallMap;
        public float cellSize; // 타일 1칸 크기 (보통 1f)

        public Vector2 targetPos;
        public float deltaTime;
        public float enemyRadius; 
        public float enemyRadiusSqr;

        public void Execute(int index, TransformAccess transform)
        {
            if (!canMove[index]) return;

            Vector2 myPos = transform.position;
            Vector2 collisionPush = Vector2.zero; 

            for (int i = 0; i < positions.Length; i++)
            {
                if (i == index) continue;
                
                Vector2 diff = myPos - positions[i];
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist < (enemyRadiusSqr * 4f) && sqrDist > 0.0001f) 
                {
                    float dist = Mathf.Sqrt(sqrDist);
                    float penetration = (enemyRadius * 2f) - dist; 
                    
                    collisionPush += (diff / dist) * penetration * 5f;
                }
            }

            Vector2 targetDir = (targetPos - myPos).normalized;
            Vector2 desiredVelocity = targetDir * speeds[index];

            Vector2 totalMovement = (desiredVelocity + collisionPush) * deltaTime;

            float checkOffsetX = Mathf.Sign(totalMovement.x) * enemyRadius;
            Vector2 nextPosX = myPos + new Vector2(totalMovement.x + checkOffsetX, 0);
            
            if (IsWall(nextPosX))
            {
                totalMovement.x = 0;
            }

            float checkOffsetY = Mathf.Sign(totalMovement.y) * enemyRadius;
            Vector2 nextPosY = myPos + new Vector2(0, totalMovement.y + checkOffsetY);
            
            if (IsWall(nextPosY))
            {
                totalMovement.y = 0;
            }

            transform.position = myPos + totalMovement;
        }
        private bool IsWall(Vector2 checkPos)
        {
            // 1. 검사할 월드 좌표를 그냥 글로벌 셀 좌표로 깎아버립니다.
            Vector2Int cellCoord = new Vector2Int(
                Mathf.FloorToInt(checkPos.x / cellSize),
                Mathf.FloorToInt(checkPos.y / cellSize)
            );

            // 2. 장부(해시맵)에 그 좌표가 등록되어 있는지 검사합니다. (속도: O(1))
            // 등록되어 있다면 100% 벽입니다!
            return globalWallMap.ContainsKey(cellCoord);
        }
    }
}
