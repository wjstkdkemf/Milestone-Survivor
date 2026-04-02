using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    public enum EnemyState { Idle, Escape, Chasing, Attacking, Fleeing, Stunned, Dead }
    public EnemyState currentNormalState = EnemyState.Idle;
    public EnemyRarity rarity;
    protected bool facingRight = true;//좌우 구분
    protected bool I_frame = false;//무적효과
    public bool CantBeKnocked = false;// 넉백방지
    public bool stopMoving = false;  // Flag to stop movement
    protected bool isAgentReady = false; // agent 안전용
    [SerializeField] protected float maxhealth;
    protected float health;             // Base health for the enemy
    [SerializeField] protected float damage;             // Base damage for the enemy
    [SerializeField] protected float speed;              // Movement speed for the enemy
    [SerializeField] protected float knockBackForce = 4;     // Force applied during knockback
    [SerializeField] protected float coolDown = 3;
    [SerializeField] protected float range = 5f;         // Detection range for chasing the player
    [SerializeField] protected float attackRange = 2f;   // Range within which the enemy can attack
    [SerializeField] protected float escapeRange = 1f;   // Range at which the enemy flees
    [SerializeField] protected bool canRun = false;      // Can the enemy flee?
    public GameObject DamageText;
    public Transform player;
    public PlayerHealth playerHealth;
    protected NavMeshAgent agent;
    protected float coolDownTimer;
    protected float knockBackTime = 0f;
    public float _knockBackDuration = .2f;

    // ******************** Flash Elemnts*********************
    [SerializeField] protected Material originalMaterial;
    [SerializeField] protected Material flashMaterial;
    [SerializeField] protected Color currentStateColor;
    protected float duration = .1f;
    protected SpriteRenderer spriteRenderer;
    protected Collider2D EnemyCollider2D;
    
    protected Coroutine flashRoutine;
    [SerializeField] protected bool DontUseObjectPooling;
    [SerializeField] protected bool boss;
    protected Vector3 lastVelocity;
    protected Color defaultColor = Color.white;

    [Header("Reposition Settings")]
    [SerializeField] protected float checkInterval = 2.0f; // 검사 주기 (2초)
    [SerializeField] protected float maxDistance = 30.0f;  // 이 거리를 넘으면 소환 (화면 밖)
    [SerializeField] protected float respawnRadius = 15.0f; // 플레이어 주변 재소환 반경
    [SerializeField] protected float minimumMoveDistance = 0.5f; // 이 거리 이하로 움직이면 끼인 것으로 간주
    [SerializeField] protected int maxStuckCount = 2; // 2번 연속 제자리걸음 시 재소환 발동

    [Header("Status Effects")]
    protected float baseSpeed; // 원래 이동 속도를 기억할 변수
    protected Coroutine slowCoroutine; // 현재 실행 중인 슬로우 코루틴
    protected float maxDistanceSqr;
    protected readonly Dictionary<EnemyRarity, int> rarityMultipliers = new Dictionary<EnemyRarity, int>
    {
        { EnemyRarity.Normal, 1 },
        { EnemyRarity.Magic, 100 },
        { EnemyRarity.Rare, 200 },
        { EnemyRarity.Boss, 500 }
    };
    protected float pathUpdateTimer = 0f;
    protected float distanceCheckTimer = 0f;
    protected int stuckCount = 0;
    Vector3 lastPosition = Vector3.zero;
    
    void Awake()
    {
        GameObject gameObject = GameManager.Instance.Player;
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
        defaultColor = spriteRenderer.color;
        currentStateColor = defaultColor;

        baseSpeed = speed;
        maxDistanceSqr = maxDistance * maxDistance;
    }
    private void HandlePathUpdate()
    {
        if (!agent.enabled) return;

        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0)
        {
            if (agent.isOnNavMesh) 
            {
                agent.SetDestination(player.position);
            }
            pathUpdateTimer = Random.Range(0.2f, 0.3f); 
        }
    }

    private void HandleDistanceCheck(float distanceSqrToPlayer)
    {
        distanceCheckTimer -= Time.deltaTime;
        if (distanceCheckTimer <= 0)
        {
            // 화면 밖으로 너무 멀어지면 텔레포트
            if (distanceSqrToPlayer > maxDistanceSqr)
            {
                RepositionEnemy();
                
                stuckCount = 0;
            }
            else
            {
                if (knockBackTime > 0 || stopMoving)
                {
                    lastPosition = transform.position; // 억울하게 카운트 먹지 않도록 현재 위치만 갱신
                    stuckCount = 0;
                }

                // 제자리걸음을 하고 있는가? (벽 끼임, 길막 감지)
                float movedDistSqr = (transform.position - lastPosition).sqrMagnitude;

                if (movedDistSqr < minimumMoveDistance * minimumMoveDistance)
                {
                    stuckCount++; // 경고 누적
                    
                    if (stuckCount >= maxStuckCount)
                    {
                        Debug.Log($"[{gameObject.name}] 몬스터가 길막/벽끼임으로 인해 텔레포트합니다!");
                        RepositionEnemy();
                        
                        stuckCount = 0; // 리셋
                    }
                }
                else
                {
                    stuckCount = 0; 
                }

                lastPosition = transform.position;
            }

            distanceCheckTimer = checkInterval;
        }
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
                
                isAgentReady = true;
                yield break;
            }
        }
        Die();
    }
    void RepositionEnemy()
    {
        Vector2 randomPoint = Random.insideUnitCircle.normalized * respawnRadius;
        Vector3 potentialPos = player.position + new Vector3(randomPoint.x, randomPoint.y, 0);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(potentialPos, out hit, 5.0f, NavMesh.AllAreas))
        {
            agent.enabled = false;
            transform.position = hit.position;
            
            agent.enabled = true;
            agent.Warp(hit.position); 

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
                
                isAgentReady = true; 
                Debug.Log($"[{gameObject.name}] 재소환 및 동기화 성공");
            }
            else
            {
                isAgentReady = false;
                StartCoroutine(EnableAgentAndFollow());
                Debug.LogWarning($"[{gameObject.name}] Warp 실패. 초기화 루틴 재실행");
            }
        }
    }

    public virtual void OnEnable()
    {
        health = maxhealth;
        isAgentReady = false;
        currentNormalState = EnemyState.Chasing;

        spriteRenderer.material = originalMaterial;
        spriteRenderer.color = defaultColor;
        GameManager.Instance.activeEnemies++;
        agent.enabled = false;

        lastPosition = transform.position;
        stuckCount = 0;

        pathUpdateTimer = Random.Range(0.0f, 0.5f);
        distanceCheckTimer = Random.Range(0.0f, checkInterval);

        if (player != null)
        {
            StartCoroutine(EnableAgentAndFollow());
        }

        ResetStatusEffects();
    }
    public virtual void OnDisable()
    {
        if (health > 0) // 피가 남았는데 꺼진 경우만 추적
        {
            Debug.LogWarning($"[Enemy CSI] {gameObject.name} 비정상 종료! (Health: {health})\n호출 경로:\n{System.Environment.StackTrace}");
        }

        spriteRenderer.color = defaultColor;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.activeEnemies--;
        }
        StopAllCoroutines();
        currentNormalState = EnemyState.Dead;
    }
    void Start()
    {
      
    }

    public abstract void Attack();

    public virtual void Die()
    {
        currentNormalState = EnemyState.Dead;
        GameManager.Instance.NumberOfKills++;

        LootDrop lootDrop = GetComponent<LootDrop>();
        if (lootDrop != null)
        {
            lootDrop.DropLoot();
        }

        if (DontUseObjectPooling == false)
        {
            ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
      
    }

    protected virtual void Update()
    {
        if (currentNormalState == EnemyState.Dead || player == null || stopMoving || isAgentReady == false) return;

        knockBackTime -= Time.deltaTime;
        coolDownTimer -= Time.deltaTime;

        Vector3 delta = player.position - transform.position;
        UpdateFacingDirection(delta);

        //float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceSqrToPlayer = delta.sqrMagnitude;

        HandleDistanceCheck(distanceSqrToPlayer);
        //HandlePathUpdate();

        DetermineState(distanceSqrToPlayer);
        HandleMovement(distanceSqrToPlayer, delta);
    }

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
        if (distanceToPlayer <= attackRange * attackRange)
        {
            currentNormalState = EnemyState.Attacking;
        }
        else if (canRun && distanceToPlayer <= escapeRange * escapeRange)
        {
            currentNormalState = EnemyState.Escape;
        }
        else
        {
            // 공격 범위도 아니고, 도망갈 거리도 아니면 '추적' 해야 합니다!
            currentNormalState = EnemyState.Chasing;
        }
    }

    protected virtual void HandleMovement(float distanceToPlayer, Vector3 delta)
    {
        if (knockBackTime > 0 && !CantBeKnocked)
        {
            float finalKnockBack = knockBackForce * (1 + PlayerStats.Instance.KnockBackBonus);
            transform.position = Vector2.MoveTowards(transform.position, player.position, -1 * finalKnockBack * Time.deltaTime);
        }
        else
        {
            switch (currentNormalState)
            {
                case EnemyState.Chasing:
                    //transform.position += agent.velocity * Time.deltaTime;
                    Vector3 directMoveDir = delta.normalized;
                    transform.position += directMoveDir * speed * Time.deltaTime;
                    break;

                case EnemyState.Attacking:
                    if (coolDownTimer <= 0)
                    {
                        Attack();
                        coolDownTimer = coolDown;
                    }
                    break;

                case EnemyState.Idle:
                    transform.position += lastVelocity * Time.deltaTime;
                    break;
            }
        }

        if (agent.velocity.sqrMagnitude > 0.1f)
            lastVelocity = agent.velocity;
        if (Vector3.Distance(transform.position, agent.nextPosition) > 1.0f)
            agent.nextPosition = transform.position;
    }

    public virtual void TakeDamage(float amount, float knockBackDuration = .2f)
    {
        if (I_frame || currentNormalState == EnemyState.Dead)
        {
            return;
        }

        // Play hurt sound (if you have an audio manager)
        // AudioManager.instance.PlaySound("Enemy_Hurt");


        if (DamageText != null)
        {
            GameObject textObj = ObjectPoolingManager.Instance.spawnGameObject(DamageText, transform.position, Quaternion.identity);
            if (textObj != null)
            {
                textObj.GetComponent<TMP_Text>().text = amount.ToString();
            }
        }

        if (flashMaterial != null && !boss)
            Flash();

        health -= amount;
        // Debug.Log($"{gameObject.name} took {amount} damage, current health: {health}");

        if (health <= 0)
        {
            Die();
        }

        knockBackTime = _knockBackDuration;
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
        spriteRenderer.color = currentStateColor;

        flashRoutine = null;
    }
    public virtual void ApplySlow(float slowPercent)
    {
        // 무적 상태거나 이미 죽었으면 무시
        if (I_frame || currentNormalState == EnemyState.Dead || health <= 0) return;

        // 이미 슬로우가 걸려있다면 기존 타이머를 취소합니다.
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        float multiplier = 1f - Mathf.Clamp01(slowPercent);
        speed = baseSpeed * multiplier;
        
        if (agent != null) agent.speed = speed; // NavMesh 속도도 같이 변경

        currentStateColor = new Color(0.5f, 0.5f, 1f);
        // 시각적 효과: 파란색으로 변하게 하기
        if (spriteRenderer != null) spriteRenderer.color = currentStateColor;
    }
    public virtual void ApplySlow(float slowPercent, float duration)
    {
        // 무적 상태거나 이미 죽었으면 무시
        if (I_frame || currentNormalState == EnemyState.Dead || health <= 0) return;

        // 이미 슬로우가 걸려있다면 기존 타이머를 취소합니다.
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration));
    }
    private IEnumerator SlowRoutine(float slowPercent, float duration)
    {
        //속도 감소 적용
        float multiplier = 1f - Mathf.Clamp01(slowPercent);
        speed = baseSpeed * multiplier;
        
        if (agent != null) agent.speed = speed; // NavMesh 속도도 같이 변경

        currentStateColor = new Color(0.5f, 0.5f, 1f);
        // 시각적 효과: 파란색으로 변하게 하기
        if (spriteRenderer != null) spriteRenderer.color = currentStateColor;

        // 지속 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // 지속 시간이 끝나면 원상 복구
        ResetStatusEffects();
    }
    public void ResetStatusEffects()
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }
        // 속도 원상 복구
        speed = baseSpeed;
        if (agent != null) agent.speed = baseSpeed;
        currentStateColor = defaultColor;
        if (spriteRenderer != null) spriteRenderer.color = currentStateColor;
    }
    public void OnNavMeshUpdated()
    {
        if (currentNormalState == EnemyState.Dead || stopMoving) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.enabled = false;
            transform.position = hit.position;
            
            agent.enabled = true;
            agent.Warp(hit.position);

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                if (player != null) agent.SetDestination(player.position);
                isAgentReady = true; 
            }
            else
            {
                isAgentReady = false;
            }
        }
        else
        {
            RepositionEnemy(); 
        }
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
