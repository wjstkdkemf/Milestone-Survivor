using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinArcherScript : Enemy
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
    protected override void Update()
    {
        base.Update(); // 부모의 상태 결정 및 쿨타임 로직 실행

        // 방향 전환 타이머
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0)
        {
            // 랜덤하게 방향 전환 (50% 확률)
            strafeDir = Random.value > 0.5f ? 1 : -1;
            // 다음 전환 시간 랜덤 설정 (1초 ~ 설정값)
            strafeTimer = Random.Range(1.0f, changeDirectionInterval);
        }
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

        GameObject arrow = ObjectPoolingManager.Instance.spawnGameObject(arrowPrefab, firePoint.position, Quaternion.identity);
        EnemyProjectile arrowScript = arrow.GetComponent<EnemyProjectile>();

        if (arrowScript != null)
        {
            arrowScript.Setup(player, arrowSpeed, damage);
        }
        else
        {
            Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (player.position - firePoint.position).normalized;
                rb.velocity = direction * arrowSpeed;
                
                // 회전도 맞춰줌
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        Debug.Log(gameObject.name + " fires an arrow with " + this.damage + " damage.");
    }
    protected override void HandleMovement(float distanceToPlayer, Vector3 delta)
    {
        // 넉백 중이거나 행동 불가 상태면 부모 로직 따름
        if ((knockBackTime > 0 && !CantBeKnocked) || !IsActived) 
        {
            base.HandleMovement(distanceToPlayer, delta);
            return;
        }

        // 공격 사거리 안에 들어왔을 때
        if (distanceToPlayer <= attackRange)
        {
            Vector3 finalMoveDir = Vector3.zero;
            Vector3 dirToPlayer = delta.normalized; // 나 -> 플레이어 방향

            // 거리 조절
            if (distanceToPlayer < minMaintainDistance)
            {
                // 플레이어 반대 방향으로 도망
                finalMoveDir -= dirToPlayer; 
            }
            // 좌우 무빙
            // 플레이어 방향의 수직 벡터 구하기 (-y, x) = 2D에서의 수직
            Vector3 perpendicularDir = new Vector3(-dirToPlayer.y, dirToPlayer.x, 0);
            
            // 수직 방향 * 랜덤 방향(좌/우)
            finalMoveDir += perpendicularDir * strafeDir;

            // 정규화 후 속도 적용
            finalMoveDir.Normalize();

            // 실제 이동 적용
            transform.position += finalMoveDir * speed * Time.deltaTime;

            // 이동 중에도 플레이어를 바라보게 함
            UpdateFacingDirection(delta);

            // 공격 쿨타임 체크 및 공격
            if (coolDownTimer <= 0)
            {
                Attack();
                coolDownTimer = coolDown;
            }
        }
        else
        {
            base.HandleMovement(distanceToPlayer, delta);
        }
        
        // Agent 동기화
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && Vector3.Distance(transform.position, agent.nextPosition) > 1.0f)
        {
            agent.nextPosition = transform.position;
        }
    }
}