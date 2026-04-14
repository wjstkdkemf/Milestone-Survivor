using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinArcherScript : NavEnemy
{
    [SerializeField] private float minMaintainDistance = 4.0f; // 이 거리보다 가까우면 도망감
    [SerializeField] private float changeDirectionInterval = 2.0f; // 방향 전환 주기
    //[SerializeField] private float strafeSpeed = 3.0f; // 옆으로 움직이는 속도
    private float strafeTimer;
    private int strafeDir = 1; // 1: 오른쪽, -1: 왼쪽

    [Header("Attack Settings")]
    public GameObject arrowPrefab; // 화살 프리팹
    public Transform firePoint; // 화살 발사 위치
    public float arrowSpeed = 10f; // 화살 속도

    private Animator animator;

    void Start()
    {
        attackRange = 7.0f;
        animator = GetComponent<Animator>();
    }
    public override void ManualUpdate()
    {
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0)
        {
            strafeDir = Random.value > 0.5f ? 1 : -1;
            strafeTimer = Random.Range(1.0f, changeDirectionInterval);
        }
        base.ManualUpdate(); // 부모의 상태 결정 및 쿨타임 로직 실행
    }

    public override void Attack()
    {
        if (arrowPrefab == null || firePoint == null || player == null)
        {
            Debug.LogWarning("Arrow Prefab, Fire Point, or Player not set for Goblin Archer.");
            return;
        }

        if(animator != null)
            animator.SetTrigger("Attack");

        SpawnArrow();
    }
    public void SpawnArrow()
    {
        GameObject arrow = ObjectPoolingManager.Instance.spawnGameObject(arrowPrefab, firePoint.position, Quaternion.identity);
        if (arrow == null) return;

        EnemyProjectile arrowScript = arrow.GetComponent<EnemyProjectile>();
        if (arrowScript != null)
        {
            arrowScript.Setup(player, arrowSpeed, damage);
        }
        else
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = direction * arrowSpeed;
        }
    }
    protected override void HandleNavMovement()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        agent.stoppingDistance = 0;

        float distSqr = (player.position - transform.position).sqrMagnitude;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        agent.isStopped = false;

        // 상태별 이동 로직
        if (distSqr < minMaintainDistance * minMaintainDistance)
        {
            Vector3 fleePos = transform.position - dirToPlayer * 3f;
            MoveToNavPos(fleePos);
        }
        else if (distSqr <= attackRange * attackRange)
        {
            Vector3 perpendicularDir = new Vector3(-dirToPlayer.y, dirToPlayer.x, 0);
            Vector3 strafePos = transform.position + perpendicularDir * strafeDir * 2f;
            
            MoveToNavPos(strafePos);
        }
        else
        {
            agent.SetDestination(player.position);
            //base.HandleNavMovement();
        }

        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            transform.position += agent.velocity * Time.deltaTime;
        }
        agent.nextPosition = transform.position;
    }
    private void MoveToNavPos(Vector3 targetPos)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}