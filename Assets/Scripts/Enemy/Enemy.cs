using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 🚨 UnityEngine.AI 네임스페이스 삭제 완료 (NavMesh 완전 독립)

public abstract class Enemy : MonoBehaviour, IDamageable
{
    public enum EnemyState { Idle, Escape, Chasing, Attacking, Fleeing, Stunned, Dead }
    public enum AnimState { Run, Attack, Die }
    
    public EnemyState currentNormalState = EnemyState.Idle;
    public EnemyRarity rarity;
    protected bool facingRight = true;
    protected bool I_frame = false;
    public bool CantBeKnocked = false;
    public bool stopMoving = false; 

    [Header("Stats")]
    [SerializeField] protected float maxhealth;
    protected float health;            
    [SerializeField] protected float damage;            
    [SerializeField] protected float speed;              
    [SerializeField] protected float knockBackForce = 4;     
    [SerializeField] protected float coolDown = 3;
    [SerializeField] protected float attackRange = 2f;   
    [SerializeField] protected float escapeRange = 1f;   
    [SerializeField] protected bool canRun = false;      

    [Header("References")]
    public GameObject DamageText;
    public Transform player;
    public PlayerHealth playerHealth;
    
    protected float coolDownTimer;
    protected float knockBackTime = 0f;
    public float _knockBackDuration = .2f;

    [Header("Flash Elements")]
    [SerializeField] protected Material originalMaterial;
    [SerializeField] protected Material flashMaterial;
    [SerializeField] protected Color currentStateColor;
    protected float duration = .1f;
    protected SpriteRenderer spriteRenderer;
    protected Collider2D EnemyCollider2D;
    private Rigidbody2D EnemyRigidbody2D; 
    protected Coroutine flashRoutine;
    
    [SerializeField] protected bool DontUseObjectPooling;
    [SerializeField] protected bool boss;
    protected Vector3 lastVelocity;
    protected Color defaultColor = Color.white;
    protected AnimState currentState = AnimState.Run;

    [Header("Reposition Settings")]
    [SerializeField] protected float checkInterval = 2.0f; 
    [SerializeField] protected float maxDistance = 30.0f;  
    [SerializeField] protected float respawnRadius = 15.0f; 
    [SerializeField] protected float minimumMoveDistance = 0.5f; 
    [SerializeField] protected int maxStuckCount = 2; 

    [Header("Status Effects")]
    protected float baseSpeed; 
    protected Coroutine slowCoroutine; 
    protected float maxDistanceSqr;
    
    protected readonly Dictionary<EnemyRarity, int> rarityMultipliers = new Dictionary<EnemyRarity, int>
    {
        { EnemyRarity.Normal, 1 }, { EnemyRarity.Magic, 100 }, { EnemyRarity.Rare, 200 }, { EnemyRarity.Boss, 500 }
    };

    [Header("Soft Separation")]
    public LayerMask enemyLayer; 
    public float separationRadius = 0.5f; 
    public float separationForce = 2.0f; 
    
    private Collider2D[] neighbors = new Collider2D[5];
    private Vector3 cachedPushVector = Vector3.zero;
    private float separationTimer = 0f;
    
    protected float distanceCheckTimer = 0f;
    protected int stuckCount = 0;
    private Vector3 lastPosition = Vector3.zero;

    void Awake()
    {
        GameObject gameObject = GameManager.Instance.Player;
        player = gameObject.transform.Find("CenterPosition").transform;
        playerHealth = gameObject.GetComponent<PlayerHealth>();

        if (boss)
        {
            spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); 
            originalMaterial = spriteRenderer.material;      
            EnemyCollider2D = GetComponent<Collider2D>();
            EnemyRigidbody2D = GetComponent<Rigidbody2D>();
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); 
            originalMaterial = spriteRenderer.material;      
            EnemyRigidbody2D = GetComponent<Rigidbody2D>();
            EnemyCollider2D = GetComponent<Collider2D>();
        }
        
        defaultColor = spriteRenderer.color;
        currentStateColor = defaultColor;

        baseSpeed = speed;
        maxDistanceSqr = maxDistance * maxDistance;
    }

    protected virtual void OnEnable()
    {
        if (EnemyRigidbody2D != null) 
        {
            EnemyRigidbody2D.simulated = true; 
        }

        health = maxhealth;
        currentNormalState = EnemyState.Chasing;

        spriteRenderer.material = originalMaterial;
        spriteRenderer.color = defaultColor;
        GameManager.Instance.activeEnemies++;

        lastPosition = transform.position;
        stuckCount = 0;
        distanceCheckTimer = Random.Range(0.0f, checkInterval);
        separationTimer = Random.Range(0.0f, 0.25f);

        ResetStatusEffects();

        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.RegisterEnemy(this);
    }

    public virtual void OnDisable()
    {
        if (health > 0) 
        {
            Debug.LogWarning($"[Enemy CSI] {gameObject.name} 비정상 종료! (Health: {health})");
        }

        spriteRenderer.color = defaultColor;
        if (GameManager.Instance != null)
            GameManager.Instance.activeEnemies--;
            
        StopAllCoroutines();
        currentNormalState = EnemyState.Dead;

        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.UnregisterEnemy(this);
    }

    protected virtual void RepositionEnemy()
    {
        Vector2 randomPoint = Random.insideUnitCircle.normalized * respawnRadius;
        transform.position = player.position + new Vector3(randomPoint.x, randomPoint.y, 0);
        stuckCount = 0;
        Debug.Log($"[{gameObject.name}] 화면 밖/길막으로 인해 플레이어 주변으로 재소환됨.");
    }

    private void HandleDistanceCheck(float distanceSqrToPlayer)
    {
        distanceCheckTimer -= Time.deltaTime;
        if (distanceCheckTimer <= 0)
        {
            if (distanceSqrToPlayer > maxDistanceSqr)
            {
                RepositionEnemy();
            }
            else
            {
                if (knockBackTime > 0 || stopMoving)
                {
                    lastPosition = transform.position; 
                    stuckCount = 0;
                }

                float movedDistSqr = (transform.position - lastPosition).sqrMagnitude;

                if (movedDistSqr < minimumMoveDistance * minimumMoveDistance)
                {
                    stuckCount++; 
                    if (stuckCount >= maxStuckCount)
                    {
                        RepositionEnemy();
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

    public virtual void ManualUpdate()
    {
        if (currentNormalState == EnemyState.Dead || player == null || stopMoving) return;

        knockBackTime -= Time.deltaTime;
        coolDownTimer -= Time.deltaTime;

        Vector3 delta = player.position - transform.position;
        UpdateFacingDirection(delta);

        float distanceSqrToPlayer = delta.sqrMagnitude;

        HandleDistanceCheck(distanceSqrToPlayer);
        DetermineState(distanceSqrToPlayer);
        HandleMovement(distanceSqrToPlayer, delta);
        
    }

    protected virtual void UpdateFacingDirection(Vector3 delta)
    {
        if (delta.x >= 0 && !facingRight)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = false; 
            facingRight = true;
        }
        else if (delta.x < 0 && facingRight)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = true; 
            facingRight = false;
        }
    }

    protected virtual void DetermineState(float distanceSqrToPlayer)
    {
        if (distanceSqrToPlayer <= attackRange * attackRange)
        {
            currentNormalState = EnemyState.Attacking;
        }
        else if (canRun && distanceSqrToPlayer <= escapeRange * escapeRange)
        {
            currentNormalState = EnemyState.Escape;
        }
        else
        {
            currentNormalState = EnemyState.Chasing;
        }
    }

protected virtual void HandleMovement(float distanceSqrToPlayer, Vector3 delta)
    {
        if (knockBackTime > 0 && !CantBeKnocked)
        {
            float finalKnockBack = knockBackForce * (1 + PlayerStats.Instance.KnockBackBonus);
            Vector3 knockbackDir = -delta.normalized;
            EnemyRigidbody2D.velocity = knockbackDir * finalKnockBack;
            return; 
        }

        switch (currentNormalState)
        {
            case EnemyState.Chasing:
                Vector3 targetDirection = delta.normalized;
                Vector3 randomWobble = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);
                Vector3 swarmDirection = (targetDirection + randomWobble).normalized;

                EnemyRigidbody2D.velocity = swarmDirection * speed;
                break;

            case EnemyState.Attacking:
                EnemyRigidbody2D.velocity = Vector3.zero; 
                if (coolDownTimer <= 0)
                {
                    Attack();
                    coolDownTimer = coolDown;
                }
                break;

            case EnemyState.Idle:
                EnemyRigidbody2D.velocity = Vector3.Lerp(EnemyRigidbody2D.velocity, Vector3.zero, Time.deltaTime * 5f);
                break;
        }

        if (EnemyRigidbody2D.velocity.sqrMagnitude > 0.1f)
            lastVelocity = EnemyRigidbody2D.velocity;
    }

    protected Vector3 GetSoftSeparationVector()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, separationRadius, neighbors, enemyLayer);
        Vector3 pushVector = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            if (neighbors[i].gameObject == this.gameObject) continue;

            Vector3 diff = transform.position - neighbors[i].transform.position;
            float sqrDist = diff.sqrMagnitude; 

            if (sqrDist > 0.0001f && sqrDist < separationRadius * separationRadius)
            {
                float dist = Mathf.Sqrt(sqrDist);
                float pushStrength = 1.0f - (dist / separationRadius); 
                pushVector += diff.normalized * pushStrength;
            }
        }
        return pushVector;
    }

    public abstract void Attack();

    public virtual void TakeDamage(float amount, float knockBackDuration = .2f)
    {
        if (I_frame || currentNormalState == EnemyState.Dead) return;

        if (DamageText != null)
        {
            GameObject textObj = ObjectPoolingManager.Instance.spawnGameObject(DamageText, transform.position, Quaternion.identity);
            if (textObj != null)
            {
                textObj.GetComponent<TMP_Text>().text = amount.ToString();
            }
        }

        if (flashMaterial != null && !boss) Flash();

        health -= amount;

        if (health <= 0) Die();

        knockBackTime = _knockBackDuration;
    }

    public virtual void Die()
    {
        currentNormalState = EnemyState.Dead;

        if (EnemyRigidbody2D != null) 
        {
            EnemyRigidbody2D.simulated = false; 
        }
        GameManager.Instance.NumberOfKills++;

        LootDrop lootDrop = GetComponent<LootDrop>();
        if (lootDrop != null) lootDrop.DropLoot();

        if (!DontUseObjectPooling)
            ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
        else
            Destroy(gameObject);
    }

    public void Flash()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
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
        if (I_frame || currentNormalState == EnemyState.Dead || health <= 0) return;

        if (slowCoroutine != null) StopCoroutine(slowCoroutine);

        float multiplier = 1f - Mathf.Clamp01(slowPercent);
        speed = baseSpeed * multiplier;
        
        currentStateColor = new Color(0.5f, 0.5f, 1f);
        if (spriteRenderer != null) spriteRenderer.color = currentStateColor;
    }

    public virtual void ApplySlow(float slowPercent, float duration)
    {
        if (I_frame || currentNormalState == EnemyState.Dead || health <= 0) return;

        if (slowCoroutine != null) StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration));
    }

    private IEnumerator SlowRoutine(float slowPercent, float duration)
    {
        float multiplier = 1f - Mathf.Clamp01(slowPercent);
        speed = baseSpeed * multiplier;
        
        currentStateColor = new Color(0.5f, 0.5f, 1f);
        if (spriteRenderer != null) spriteRenderer.color = currentStateColor;

        yield return new WaitForSeconds(duration);

        ResetStatusEffects();
    }

    public void ResetStatusEffects()
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }
        speed = baseSpeed;
        currentStateColor = defaultColor;
        if (spriteRenderer != null) spriteRenderer.color = currentStateColor;
    }
}
[System.Serializable]
public enum EnemyRarity { Normal, Magic, Rare, Boss }
