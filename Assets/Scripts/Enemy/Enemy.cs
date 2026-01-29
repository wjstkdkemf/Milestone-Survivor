using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    public EnemyRarity rarity;
    public float maxhealth;
    public float health;             // Base health for the enemy
    public float damage;             // Base damage for the enemy
    public float speed;              // Movement speed for the enemy
    public float knockBackForce = 4;     // Force applied during knockback
    public float coolDown = 3;
    public float range = 5f;         // Detection range for chasing the player
    public float attackRange = 2f;   // Range within which the enemy can attack
    public float escapeRange = 1f;   // Range at which the enemy flees
    public bool canRun = false;      // Can the enemy flee?
    public bool stopMoving = false;  // Flag to stop movement
    public GameObject DamageText;
    public Transform player;
    public PlayerHealth playerHealth;
    private NavMeshAgent agent;
    private float coolDownTimer;
    protected bool facingRight = true;
    protected bool chasing = false;
    protected bool running = false;
    protected bool inAttackRange = false;
    private float knockBackTime = 0f;
    public float _knockBackDuration = .2f;
    public bool CantBeKnocked = false;

    // ******************** Flash Elemnts*********************
    public Material flashMaterial;
    protected float duration = .1f;
    protected SpriteRenderer spriteRenderer;
    protected Collider2D EnemyCollider2D;
    public Material originalMaterial;
    protected Coroutine flashRoutine;
    public bool IsActived = false;
    public bool DontUseObjectPooling;
    public bool boss;
    protected bool I_frame = false;
    private Vector3 lastVelocity;
    private bool isRecovering = false;
    private Color defaultColor = Color.white;

    [Header("Reposition Settings")]
    [SerializeField] private float checkInterval = 2.0f; // 검사 주기 (2초)
    [SerializeField] private float maxDistance = 30.0f;  // 이 거리를 넘으면 소환 (화면 밖)
    [SerializeField] private float respawnRadius = 15.0f; // 플레이어 주변 재소환 반경
    private float maxDistanceSqr;
    // Multipliers based on monster rarity
    private readonly Dictionary<EnemyRarity, int> rarityMultipliers = new Dictionary<EnemyRarity, int>
    {
        { EnemyRarity.Normal, 1 },
        { EnemyRarity.Magic, 100 },
        { EnemyRarity.Rare, 200 },
        { EnemyRarity.Boss, 500 }
    };
    
    void Awake()
    {
        GameObject gameObject = GameObject.FindWithTag("Player");
        player = gameObject.transform.Find("CenterPosition").transform;
        playerHealth = gameObject.GetComponent<PlayerHealth>();

        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.updatePosition = false;
        agent.acceleration = 100f; 
        agent.autoBraking = false; 

        defaultColor = spriteRenderer.color; 

        if (boss)
        {
            spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); // Initialize the spriteRenderer
            originalMaterial = spriteRenderer.material;      // Initialize the original material
            EnemyCollider2D = GetComponent<Collider2D>();
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); // Initialize the spriteRenderer
            originalMaterial = spriteRenderer.material;      // Initialize the original material
            EnemyCollider2D = GetComponent<Collider2D>();
        }
        maxDistanceSqr = maxDistance * maxDistance;
    }
    IEnumerator EnableAgentAndFollow()
    {
        yield return new WaitForSeconds(Random.Range(0.0f, 0.2f));

        int maxRetries = 10; 
        float searchRadius = 5.0f;

        for (int i = 0; i < maxRetries; i++)
        {
            // 맵 생성 직후 프레임 대기
            yield return new WaitForSeconds(0.1f);
            if (!gameObject.activeInHierarchy) yield break;

            NavMeshHit hit;
            // 내 위치 주변에 NavMesh가 있는지 확인
            // SamplePosition(중심점, 결과저장변수, 반경, 영역마스크)
            if (NavMesh.SamplePosition(transform.position, out hit, searchRadius, NavMesh.AllAreas)) 
            {
                // 찾았다면 해당 위치로 순간이동(Warp) 후 활성화
                agent.Warp(hit.position); 
                agent.enabled = true;
                
                // 추적 루틴 시작
                StartCoroutine(UpdatePathRoutine());
                yield break;
            }
        }
        Die();
    }
    IEnumerator CheckDistanceRoutine()
    {
        // 1. 모든 몬스터가 동시에 연산하지 않도록 랜덤 딜레이를 줍니다. (부하 분산)
        yield return new WaitForSeconds(Random.Range(0f, checkInterval));

        while (player != null)
        {
            // 2. 설정한 주기만큼 대기 (Update보다 훨씬 가벼움)
            yield return new WaitForSeconds(checkInterval);

            if (!gameObject.activeInHierarchy || !agent.enabled) continue;

            // 3. 거리 계산 (sqrMagnitude 사용으로 최적화)
            float distSqr = (player.position - transform.position).sqrMagnitude;

            // 4. 제한 거리를 넘었다면?
            if (distSqr > maxDistanceSqr)
            {
                RepositionEnemy();
            }
        }
    }
    IEnumerator UpdatePathRoutine()
    {
        while (player != null && agent.enabled)
        {
            // Agent가 켜져있고, 활성화 상태일 때만 목적지 설정
            if (agent.isOnNavMesh) 
            {
                agent.SetDestination(player.position);
            }
            yield return new WaitForSeconds(Random.Range(0.2f, 0.3f));
        }
    }
    public void OnNavMeshUpdated()
    {
        // 코루틴으로 안전하게 다음 프레임에 처리
        StartCoroutine(RecoverAgent());
    }

    IEnumerator RecoverAgent()
    {
        isRecovering = true;

        if (agent.enabled) agent.enabled = false;

        yield return new WaitForSeconds(Random.Range(0.0f, 0.2f));

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 0.2f, NavMesh.AllAreas))
        {
            agent.enabled = true;
            agent.Warp(hit.position);

            yield return null;

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                if (player != null)
                {
                    agent.SetDestination(player.position);
                }
                StartCoroutine(UpdatePathRoutine());
            }
        }
        else
        {
            Vector3 rescuePosition = transform.position;
            bool foundRescuePoint = false;

            if (player != null)
            {
                // 플레이어 방향 벡터
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                
                // 내 위치에서 플레이어 쪽으로 1m, 2m, 3m 떨어진 지점을 순차적으로 검사
                float[] checkDistances = { 0.5f, 1.0f, 1.5f }; 

                foreach (float dist in checkDistances)
                {
                    // 플레이어 쪽으로 dist만큼 이동한 가상의 지점
                    Vector3 checkPos = transform.position + (directionToPlayer * dist);
                    
                    // 그 지점 주변에서 NavMesh를 찾음 (반경 1.0f)
                    if (NavMesh.SamplePosition(checkPos, out hit, 1.0f, NavMesh.AllAreas))
                    {
                        rescuePosition = hit.position;
                        foundRescuePoint = true;
                        break; // 유효한 곳을 찾으면 즉시 탈출
                    }
                }
            }

            // 플레이어 방향으로 못 찾았다면, 최후의 수단으로 그냥 주변(5.0f) 검색
            if (!foundRescuePoint)
            {
                 if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
                 {
                     rescuePosition = hit.position;
                     foundRescuePoint = true;
                 }
            }

            if (foundRescuePoint)
            {
                transform.position = rescuePosition; 
                agent.enabled = true;
                agent.Warp(rescuePosition);
                
                
                yield return null;
                
                if (agent.isActiveAndEnabled && agent.isOnNavMesh && player != null)
                {
                    agent.SetDestination(player.position);
                }
                StartCoroutine(UpdatePathRoutine());
            }
            else
            {
                Die();
            }
        }
        isRecovering = false;
    }
    void RepositionEnemy()
    {
        // 플레이어 주변의 랜덤한 위치(원형) 계산
        // insideUnitCircle을 사용하여 플레이어 주변 랜덤 위치를 잡습니다.
        Vector2 randomPoint = Random.insideUnitCircle.normalized * respawnRadius;
        Vector3 potentialPos = player.position + new Vector3(randomPoint.x, randomPoint.y, 0);

        NavMeshHit hit;
        // 5. 해당 위치 근처(3.0f)에 유효한 NavMesh(길)가 있는지 확인
        if (NavMesh.SamplePosition(potentialPos, out hit, 3.0f, NavMesh.AllAreas))
        {
            // [중요] updatePosition=false를 쓰고 계시므로, 둘 다 옮겨야 합니다.
            agent.Warp(hit.position);       // Agent(영혼) 이동
            transform.position = hit.position; // 몸(Sprite) 이동
            
            // 이동 후 즉시 목적지 갱신
            agent.SetDestination(player.position);
            
            Debug.Log("몬스터가 너무 멀어져서 재소환되었습니다.");
        }
    }

    public virtual void OnEnable()
    {
        health = maxhealth;
        IsActived = true;
        spriteRenderer.material = originalMaterial;
        spriteRenderer.color = defaultColor;
        GameManager.Instance.activeEnemies++;
        agent.enabled = false;
        if (player != null)
        {
            StartCoroutine(EnableAgentAndFollow());
            StartCoroutine(CheckDistanceRoutine());
        }
    }
    public virtual void OnDisable()
    {
        if (health > 0) // 피가 남았는데 꺼진 경우만 추적
        {
            Debug.LogWarning($"[Enemy CSI] {gameObject.name} 비정상 종료! (Health: {health})\n호출 경로:\n{System.Environment.StackTrace}");
        }

        spriteRenderer.color = defaultColor;
        GameManager.Instance.activeEnemies--;
        StopAllCoroutines();
        IsActived = false;
    }
    void Start()
    {
      
    }

    public abstract void Attack();

    public virtual void Die()
    {
        IsActived = false;
        GameManager.Instance.NumberOfKills++;

        LootDrop lootDrop = GetComponent<LootDrop>();
        if (lootDrop != null)
        {
            lootDrop.DropLoot();
        }

        if (DontUseObjectPooling == false)
        {
            ObjectPoolingManager.instance.ReturnObjectToPool(gameObject);
            IsActived = false;
        }
        else
        {
            Destroy(gameObject);
            IsActived = false;
        }
      
    }

    protected virtual void Update()
    {
        if (player == null || stopMoving || IsActived == false) return;

        knockBackTime -= Time.deltaTime;
        Vector3 delta = player.position - transform.position;
        coolDownTimer -= Time.deltaTime;
        UpdateFacingDirection(delta);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        DetermineState(distanceToPlayer);

        HandleMovement(distanceToPlayer, delta);
    }

    // Updates the direction the enemy is facing
    protected virtual void UpdateFacingDirection(Vector3 delta)
    {
        if (delta.x >= 0 && !facingRight)
        {
            transform.localScale = new Vector3(1, 1, 1);  // Face right
            facingRight = true;
        }
        else if (delta.x < 0 && facingRight)
        {
            transform.localScale = new Vector3(-1, 1, 1);  // Face left
            facingRight = false;
        }
    }

    protected virtual void DetermineState(float distanceToPlayer)
    {
        chasing = true;
       
        if (distanceToPlayer <= attackRange)
        {
            inAttackRange = true;
        }
        else
        {
            inAttackRange = false;
        }

        if (canRun && distanceToPlayer <= escapeRange)
        {
            running = true;
        }
        else
        {
            running = false;
        }
    }

    protected virtual void HandleMovement(float distanceToPlayer, Vector3 delta)
    {
        if (knockBackTime > 0 && !CantBeKnocked)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, -1 * knockBackForce * Time.deltaTime);
        }
        else
        {
            if(isRecovering)
            {
                transform.position += lastVelocity * Time.deltaTime;
            }
            else if (chasing)
            {
                transform.position += agent.velocity * Time.deltaTime;
            }
            else if (running)
            {
                transform.position += agent.velocity * Time.deltaTime;
            }
            
            if (inAttackRange && coolDownTimer <= 0)
            {
                Attack();
                coolDownTimer = coolDown;
            }

            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                lastVelocity = agent.velocity;
            }
        }
        if (Vector3.Distance(transform.position, agent.nextPosition) > 1.0f)
        {
            agent.nextPosition = transform.position;
        }
    }

    public virtual void TakeDamage(float amount, float knockBackDuration = .2f)
    {
        if (I_frame)
            return;

        // Play hurt sound (if you have an audio manager)
        // AudioManager.instance.PlaySound("Enemy_Hurt");

        if (DamageText != null)
        {
            GameObject text = Instantiate(DamageText, transform.position, Quaternion.identity);
            text.GetComponent<TMP_Text>().text = amount.ToString(); // Display the amount of damage taken
        }

        if (flashMaterial != null && !boss&&IsActived)
            Flash();

        health -= amount;
        // Debug.Log($"{gameObject.name} took {amount} damage, current health: {health}");

        if (health <= 0 && IsActived)
        {
            IsActived = false;
            Die();
        }

        knockBackTime = _knockBackDuration;
    }

    public bool IsAlive()
    {
        return IsActived;
    }

    protected virtual void ApplyKnockback(Vector3 direction, float force)
    {

    }
    public void Flash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    public IEnumerator FlashRoutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = defaultColor;

        flashRoutine = null;
    }

    private Vector2 GetRandomPositionAround(Vector2 centerPosition, float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Random.Range(0f, radius);
        float x = centerPosition.x + Mathf.Cos(angle) * distance;
        float y = centerPosition.y + Mathf.Sin(angle) * distance;
        return new Vector2(x, y);
    }
}

[System.Serializable]
public enum EnemyRarity { Normal, Magic, Rare, Boss }
