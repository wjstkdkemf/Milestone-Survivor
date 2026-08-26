using UnityEngine;
using UnityEngine.AI; // NavMeshAgent 제어를 위해 필요

public class BatEnemy : Enemy
{
    [Header("Bat Movement Settings")]
    [SerializeField] private float flyDistance = 25f; // 화면을 가로질러 날아갈 총 거리
    
    private Vector3 moveDirection; // 날아갈 방향
    //private bool isSetup = false;

    protected override void OnEnable()
    {
        // 부모(Enemy)의 기본 초기화 실행 (체력 리셋 등)
        // 주의: 부모의 OnEnable에 StartCoroutine(EnableAgentAndFollow)가 있다면,
        // 박쥐는 NavMesh를 안 쓰므로 이걸 막거나 agent를 꺼야 합니다.
        // Enemy.cs 구조상 base.OnEnable()을 부르면 NavMesh 로직이 돌 수 있으니,
        // 여기서는 수동으로 필요한 것만 초기화하는 게 나을 수 있습니다.
        
        InitializeBat();
    }
    protected override void OnDisable() 
    {
        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;
        if (GameManager.Instance != null)
            GameManager.Instance.activeEnemies--;

        StopAllCoroutines();
        currentNormalState = EnemyState.Dead;
    }

    private void InitializeBat()
    {
        health = maxhealth;
        currentNormalState = EnemyState.Idle;
        CachePlayerReferences();
        if(spriteRenderer != null) 
        {
            spriteRenderer.material = originalMaterial;
            spriteRenderer.color = Color.white;
        }
        if(GameManager.Instance != null)
            GameManager.Instance.activeEnemies++;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false; 
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        // 방향 설정 (A -> B)
        // 현재 위치(A)에서 플레이어(중심)를 향하는 방향을 구합니다.
        if (player != null)
        {
            Vector3 targetPos = player.position;
            moveDirection = (targetPos - transform.position).normalized;
            
            // 바라보는 방향 설정
            UpdateFacingDirection(player.position - transform.position);
        }
        else
        {
            moveDirection = Vector3.right; // 플레이어 없으면 오른쪽으로
        }

        //isSetup = true;
    }

    public override void ManualUpdate()
    {
        if (currentNormalState == EnemyState.Dead || stopMoving) return;

        transform.position += moveDirection * speed * Time.deltaTime;

        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > flyDistance) // 화면 밖 소멸 거리
            {
                gameObject.SetActive(false);
            }
        }
    }

    // 공격 구현 (Enemy 추상 클래스 요구사항)
    public override void Attack()
    {
    }

    protected override void HandleMovement(float distanceToPlayer, Vector3 delta)
    {
        // 부모의 HandleMovement를 안 쓰기 때문에 비워둠
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (currentNormalState == EnemyState.Dead) return;

        if (collision.CompareTag("Player"))
        {
            IDamageable playerHealth = collision.GetComponent<IDamageable>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}