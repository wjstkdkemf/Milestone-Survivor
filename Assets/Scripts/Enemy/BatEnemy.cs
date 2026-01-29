using UnityEngine;
using UnityEngine.AI; // NavMeshAgent 제어를 위해 필요

public class BatEnemy : Enemy
{
    [Header("Bat Movement Settings")]
    [SerializeField] private float flyDistance = 25f; // 화면을 가로질러 날아갈 총 거리
    
    private Vector3 moveDirection; // 날아갈 방향
    private bool isSetup = false;

    // 1. 초기화 (Awake/Start 대신 OnEnable 활용)
    // 부모의 OnEnable도 실행해야 하므로 base.OnEnable() 호출 필수
    private void OnEnable()
    {
        // 부모(Enemy)의 기본 초기화 실행 (체력 리셋 등)
        // 주의: 부모의 OnEnable에 StartCoroutine(EnableAgentAndFollow)가 있다면,
        // 박쥐는 NavMesh를 안 쓰므로 이걸 막거나 agent를 꺼야 합니다.
        // Enemy.cs 구조상 base.OnEnable()을 부르면 NavMesh 로직이 돌 수 있으니,
        // 여기서는 수동으로 필요한 것만 초기화하는 게 나을 수 있습니다.
        
        InitializeBat();
    }

    private void InitializeBat()
    {
        // 기본 스탯 초기화
        health = maxhealth;
        IsActived = true;
        if(spriteRenderer != null) 
        {
            spriteRenderer.material = originalMaterial;
            spriteRenderer.color = Color.white;
        }
        GameManager.Instance.activeEnemies++;

        // [핵심] NavMeshAgent 끄기 (박쥐는 날아다님)
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false; 
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        // [핵심] 방향 설정 (A -> B)
        // 현재 위치(A)에서 플레이어(중심)를 향하는 방향을 구합니다.
        if (player != null)
        {
            // 플레이어 방향으로 설정하되, 약간의 랜덤성을 줄 수도 있습니다.
            Vector3 targetPos = player.position;
            moveDirection = (targetPos - transform.position).normalized;
            
            // 바라보는 방향(Sprite) 설정
            UpdateFacingDirection(player.position - transform.position);
        }
        else
        {
            moveDirection = Vector3.right; // 플레이어 없으면 오른쪽으로
        }

        isSetup = true;
    }

    // 2. 부모의 Update를 완전히 덮어씀 (override)
    // 기존의 NavMesh 추적 로직을 싹 무시하고 직선 이동만 수행합니다.
    protected override void Update()
    {
        if (!IsActived || stopMoving) return;

        // [이동 로직] 방향대로 등속 운동
        transform.position += moveDirection * speed * Time.deltaTime;

        // [방향 전환] 한 번 정해진 방향으로 가지만, 혹시 필요하다면 여기서 처리
        // (박쥐는 보통 방향을 안 바꿉니다)

        // [소멸 로직] 플레이어와 너무 멀어지면(화면 밖으로 나가면) 사라짐
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > flyDistance) // 화면 밖 소멸 거리
            {
                // Die() 대신 조용히 사라지기 (킬 카운트 안 올림)
                gameObject.SetActive(false);
            }
        }
        
        // 공격 로직 (충돌은 OnCollision/OnTrigger에서 처리하거나 거리 기반으로)
        // 박쥐는 보통 몸통 박치기이므로 별도의 Attack() 호출 없이 Collider 충돌로 처리하는 게 빠름
    }

    // 공격 구현 (Enemy 추상 클래스 요구사항)
    public override void Attack()
    {
        // 박쥐는 멈춰서 때리지 않고 그냥 지나가면서 데미지를 줌 (충돌 데미지)
        // 따라서 여기는 비워두거나 특수 패턴용으로 씁니다.
    }

    // 넉백 처리 (박쥐는 넉백을 적게 받거나 안 받을 수도 있음)
    protected override void HandleMovement(float distanceToPlayer, Vector3 delta)
    {
        // 부모의 HandleMovement를 안 쓰기 때문에 비워둠
    }
    
    // 만약 충돌 데미지를 주고 싶다면 (이전 대화의 '방법 2' 추천)
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsActived) return;

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