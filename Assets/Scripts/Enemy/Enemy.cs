using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    public enum EnemyState { Idle, Escape, Chasing, Attacking, Fleeing, Stunned, Dead }
    public enum AnimState { Run, Attack, Die }

    public string enemyID;
    public EnemyState currentNormalState = EnemyState.Idle;
    public EnemyRarity rarity;
    protected bool facingRight = true;
    protected bool I_frame = false;
    public bool CantBeKnocked = false;
    protected bool previousCantBeKnocked = false;
    public bool stopMoving = false; 
    public bool useSwarmMovement = true;
    public bool casting = false;
    private bool repositionPending;
    private int repositionRequestVersion;
    private float nextRepositionRequestTime;
    public uint SpawnVersion = 0;

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
    [Header("Collision")]
    [SerializeField, Min(0.05f)]
    private float collisionRadius = 0.5f;

    public float CollisionRadius => collisionRadius;
    public virtual bool RequiresNavMesh => false;

    [Header("References")]
    public GameObject DamageText;
    public Transform player;
    public PlayerHealth playerHealth;
    private LootDrop lootDrop;
    
    protected float coolDownTimer;
    protected float knockBackTime = 0f;
    private Vector2 knockbackDirection;
    private float currentKnockbackSpeed;
    public float _knockBackDuration = .2f;

    [Header("Flash Elements")]
    [SerializeField] protected Material originalMaterial;
    [SerializeField] protected Material flashMaterial;
    [SerializeField] protected Color currentStateColor;
    protected float duration = .1f;
    protected SpriteRenderer spriteRenderer;
    //protected Collider2D EnemyCollider2D;
    //private Rigidbody2D EnemyRigidbody2D;
    protected Vector3 velocity;
    protected Coroutine flashRoutine;
    
    [SerializeField] protected bool DontUseObjectPooling;
    [SerializeField] protected bool boss;
    protected Vector3 lastVelocity;
    protected Color defaultColor = Color.white;
    protected AnimState currentState = AnimState.Run;
    protected float slowEndTime = 0f;       // 슬로우가 끝나는 시간
    protected float currentSlowPercent = 0f;
    protected bool isSlowed = false;        // 현재 슬로우 상태인지 여부

    [Header("Reposition Settings")]
    [SerializeField] protected float checkInterval = 2.0f; 
    [SerializeField] protected float maxDistance = 30.0f;  
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
    protected float distanceCheckTimer = 0f;
    protected float flashTimer = 0f;
    protected int stuckCount = 0;
    private Vector3 lastPosition = Vector3.zero;
    public float separationWeight = 2.5f;
    public float targetWeight = 1.0f;
    public float maxForce = 10f;
    public float neighborRadius = 1.2f;
    public Vector2Int currentCell;

    private static List<Enemy> nearbyEnemies = new List<Enemy>(32);

    protected virtual void Awake()
    {
        CachePlayerReferences();
        lootDrop = GetComponent<LootDrop>();


        if (boss)
        {
            if (transform.childCount > 0)
                spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); 
            //EnemyCollider2D = GetComponent<Collider2D>();
           // EnemyRigidbody2D = GetComponent<Rigidbody2D>();
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); 
            //EnemyRigidbody2D = GetComponent<Rigidbody2D>();
            //EnemyCollider2D = GetComponent<Collider2D>();
        }

        if (spriteRenderer != null)
        {
            if (originalMaterial == null)
                originalMaterial = spriteRenderer.sharedMaterial;
        
            defaultColor = spriteRenderer.color;
        }

        currentStateColor = defaultColor;

        baseSpeed = speed;
        maxDistanceSqr = maxDistance * maxDistance;
    }

    protected virtual void OnEnable()
    {
       // if (EnemyRigidbody2D != null) 
       // {
       //     EnemyRigidbody2D.simulated = true; 
       // }
        InitializeRuntimeState();
        RegisterSystem();
    }

    protected virtual void OnDisable()
    {
        ReSetRuntimeState();
        UnRegisterSystem();
    }
    protected virtual void InitializeRuntimeState()
    {
        health = maxhealth;
        currentNormalState = EnemyState.Chasing;
        ResetKnockback();
        SpawnVersion++;
        CachePlayerReferences();
        if (spriteRenderer != null)
        {
            if (originalMaterial != null)
                spriteRenderer.sharedMaterial = originalMaterial;

            spriteRenderer.color = defaultColor;
        }
        lastPosition = transform.position;
        stuckCount = 0;
        distanceCheckTimer = Random.Range(0.0f, checkInterval);
        InvalidateRepositionRequest();
        nextRepositionRequestTime = 0f;

        ResetStatusEffects();
        if (GameManager.Instance != null)
            GameManager.Instance.activeEnemies++;
    }
    protected virtual void ReSetRuntimeState()
    {
        InvalidateRepositionRequest();
        ResetKnockback();

        if (health > 0) 
        {
            Debug.LogWarning($"[Enemy CSI] {gameObject.name} 비정상 종료! (Health: {health})");
        }

        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;

        if (GameManager.Instance != null)
            GameManager.Instance.activeEnemies--;
            
        StopAllCoroutines();
        currentNormalState = EnemyState.Dead;
    }
    protected virtual void RegisterSystem()
    {       
        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.RegisterEnemy(this);
        if (EnemySwarmSystem.Instance != null )//&& useSwarmMovement
            EnemySwarmSystem.Instance.RegisterEnemy(this);
    }
    protected virtual void UnRegisterSystem()
    {
        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.UnregisterEnemy(this);
        if (EnemySwarmSystem.Instance != null)
            EnemySwarmSystem.Instance.UnregisterEnemy(this);
    }
    public float GetKnockBackTime() { return knockBackTime; }
    public float GetSpeed() { return speed; }

    public void ApplyKnockback(Vector2 origin, float speed, float duration)
    {
        if (!isActiveAndEnabled || CantBeKnocked || I_frame ||
            currentNormalState == EnemyState.Dead || health <= 0f ||
            speed <= 0f || duration <= 0f)
        {
            return;
        }

        float bonus = PlayerStats.Instance != null ? PlayerStats.Instance.KnockBackBonus : 0f;
        float finalSpeed = speed * Mathf.Max(0f, 1f + bonus);
        if (finalSpeed <= 0f) return;

        // A weaker hit must not replace an active barrier knockback.
        if (knockBackTime > 0f && finalSpeed < currentKnockbackSpeed)
            return;

        Vector2 direction = (Vector2)transform.position - origin;
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        knockbackDirection = direction.normalized;
        currentKnockbackSpeed = finalSpeed;
        knockBackTime = Mathf.Max(knockBackTime, duration);
    }

    protected void MoveKnockback()
    {
        transform.position += (Vector3)(knockbackDirection * currentKnockbackSpeed * Time.deltaTime);
    }

    protected void ResetKnockback()
    {
        knockBackTime = 0f;
        knockbackDirection = Vector2.zero;
        currentKnockbackSpeed = 0f;
    }

    protected bool CachePlayerReferences()
    {
        if (player != null && playerHealth != null)
            return true;

        if (GameManager.Instance == null || GameManager.Instance.Player == null)
            return false;

        GameObject playerObject = GameManager.Instance.Player;

        if (player == null)
        {
            Transform center = playerObject.transform.Find("CenterPosition");
            player = center != null ? center : playerObject.transform;
        }

        if (playerHealth == null)
            playerHealth = playerObject.GetComponent<PlayerHealth>();

        return player != null;
    }

    protected void RequestReposition(RepositionReason reason)
    {
        if (repositionPending ||
            Time.time < nextRepositionRequestTime)
        {
            return;
        }

        EnemyPlacementManager manager =
            EnemyPlacementManager.Instance;

        if (manager == null)
        {
            nextRepositionRequestTime =
                Time.time + checkInterval;
            return;
        }

        repositionPending = true;
        repositionRequestVersion++;

        manager.RequestReposition(
            this,
            reason,
            repositionRequestVersion);
    }

    public virtual void ApplyReposition(Vector3 position)
    {
        transform.position = position;
        FinishReposition(true);
    }

    public void FinishReposition(bool success)
    {
        repositionPending = false;

        if (success)
        {
            lastPosition = transform.position;
            stuckCount = 0;
            distanceCheckTimer = checkInterval;
            nextRepositionRequestTime = 0f;
        }
        else
        {
            nextRepositionRequestTime =
                Time.time + checkInterval;
        }
    }

    internal bool IsRepositionRequestCurrent(int version)
    {
        return repositionPending &&
               repositionRequestVersion == version;
    }

    private void InvalidateRepositionRequest()
    {
        repositionPending = false;
        repositionRequestVersion++;
    }

    protected void HandleDistanceCheck(float distanceSqrToPlayer)
    {
        distanceCheckTimer -= Time.deltaTime;
        if (distanceCheckTimer <= 0)
        {
            bool isTooFar =
                distanceSqrToPlayer > maxDistanceSqr;

            EnemyPlacementManager placementManager =
                EnemyPlacementManager.Instance;

            if (isTooFar && placementManager != null)
            {
                isTooFar =
                    placementManager.IsBeyondRecycleBounds(
                        transform.position);
            }

            if (isTooFar)
            {
                RequestReposition(RepositionReason.TooFar);
            }
            else
            {
                bool shouldExpectMovement =
                    currentNormalState == EnemyState.Chasing &&
                    knockBackTime <= 0f &&
                    !stopMoving;

                if (!shouldExpectMovement)
                {
                    lastPosition = transform.position; 
                    stuckCount = 0;
                }
                else
                {
                    float movedDistSqr =
                        (transform.position - lastPosition).sqrMagnitude;

                    if (movedDistSqr <
                        minimumMoveDistance * minimumMoveDistance)
                    {
                        stuckCount++;
                        if (stuckCount >= maxStuckCount)
                        {
                            RequestReposition(RepositionReason.Stuck);
                        }
                    }
                    else
                    {
                        stuckCount = 0;
                    }

                    lastPosition = transform.position;
                }
            }
            distanceCheckTimer = checkInterval;
        }
    }

    public virtual void ManualUpdate()
    {
        if (currentNormalState == EnemyState.Dead || stopMoving) return;
        if (player == null && !CachePlayerReferences()) return;

        knockBackTime -= Time.deltaTime;
        coolDownTimer -= Time.deltaTime;

        UpdateFlash();

        Vector3 delta = player.position - transform.position;
        UpdateFacingDirection(delta);
        CheckStatusEffects();

        float distanceSqrToPlayer = delta.sqrMagnitude;

        HandleDistanceCheck(distanceSqrToPlayer);
        DetermineState(distanceSqrToPlayer);
        //HandleMovement(distanceSqrToPlayer, delta);
        
        //transform.position += velocity * Time.deltaTime;
        //GridManager.Instance.UpdateCell(this);
        HandleAction(delta);
    }
    protected virtual void HandleAction(Vector3 delta)
    {
        // 넉백 처리 (이것은 Job이 아니라 여기서 직접 처리합니다)
        if (knockBackTime > 0 && !CantBeKnocked)
        {
            MoveKnockback();
            return;
        }

        switch (currentNormalState)
        {
            case EnemyState.Attacking:
                if (coolDownTimer <= 0)
                {
                    Attack();
                    coolDownTimer = coolDown;
                }
                break;
        }
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

    /*protected virtual void HandleMovement(float distanceSqrToPlayer, Vector3 delta)
    {
        if (knockBackTime > 0 && !CantBeKnocked)
        {
            float knockBackBonus = PlayerStats.Instance != null ? PlayerStats.Instance.KnockBackBonus : 0f;
            float finalKnockBack = knockBackForce * (1 + knockBackBonus);
            Vector3 knockbackDir = -delta.normalized;
            velocity = knockbackDir * finalKnockBack;
            return;
        }

        switch (currentNormalState)
        {
            case EnemyState.Chasing:
            {
                Vector3 targetDir = delta.normalized;

                Vector3 separation = ComputeSeparation();

                Vector3 finalDir = targetDir * targetWeight + separation * separationWeight;

                Vector3 desiredVelocity = finalDir.normalized * speed;

                velocity = Vector3.Lerp(velocity, desiredVelocity, Time.deltaTime * 6f);
                break;
            }

            case EnemyState.Attacking:
            {
                velocity = Vector3.zero;

                if (coolDownTimer <= 0)
                {
                    Attack();
                    coolDownTimer = coolDown;
                }
                break;
            }

            case EnemyState.Idle:
            {
                velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 5f);
                break;
            }
        }

        if (velocity.sqrMagnitude > 0.1f)
            lastVelocity = velocity;
    }
    */

    public abstract void Attack();

    public virtual void TakeDamage(float amount, float knockBackDuration = .2f)
    {
        if (I_frame || currentNormalState == EnemyState.Dead) return;

        if (DamageText != null)
        {
            GameObject textObj = ObjectPoolingManager.Instance != null
                ? ObjectPoolingManager.Instance.spawnGameObject(DamageText, transform.position, Quaternion.identity)
                : Instantiate(DamageText, transform.position, Quaternion.identity);

            if (textObj != null)
            {
                TMP_Text damageText = textObj.GetComponent<TMP_Text>();
                if (damageText != null)
                    damageText.text = amount.ToString();
            }
        }

        if (flashMaterial != null && !boss && spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            flashTimer = duration;
        }

        health -= amount;

        if (health <= 0)
        {
            Die();
            return;
        }

        if (player != null || CachePlayerReferences())
            ApplyKnockback(player.position, knockBackForce, _knockBackDuration);
    }

    public virtual void Die()
    {
        currentNormalState = EnemyState.Dead;

        //if (EnemyRigidbody2D != null) 
        //{
          //  EnemyRigidbody2D.simulated = false; 
        //}
        if (GameManager.Instance != null)
            GameManager.Instance.NumberOfKills++;

        GlobalEventManager.OnEnemyKilled?.Invoke(enemyID);

        if (lootDrop != null) lootDrop.DropLoot();

        if (!DontUseObjectPooling && ObjectPoolingManager.Instance != null)
            ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
        else
            Destroy(gameObject);
    }

    protected virtual void UpdateFlash()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = currentStateColor;
            }
        }
    }

    public virtual void ApplySlow(float slowPercent, float duration)
    {
        if (I_frame || currentNormalState == EnemyState.Dead || health <= 0) return;

        currentSlowPercent = Mathf.Clamp01(slowPercent);
        slowEndTime = Time.time + duration; 

        if (!isSlowed)
        {
            isSlowed = true;
            currentStateColor = new Color(0.5f, 0.5f, 1f);
            if (spriteRenderer != null) spriteRenderer.color = currentStateColor;
        }
    }
    protected virtual void CheckStatusEffects()
    {
        if (isSlowed)
        {
            // 아직 만료 시간이 안 지났다면? 속도를 깎아버립니다.
            if (Time.time < slowEndTime)
            {
                speed = baseSpeed * (1f - currentSlowPercent);
            }
            // 시간이 지났다면? 원상 복구! (Exit 이벤트나 코루틴 종료 대기 필요 없음)
            else
            {
                ResetStatusEffects();
            }
        }
    }
    public void ResetStatusEffects()
    {
        isSlowed = false;
        speed = baseSpeed;
        currentStateColor = defaultColor;
        if (spriteRenderer != null) spriteRenderer.color = currentStateColor;
    }
}
[System.Serializable]
public enum EnemyRarity { Normal, Magic, Rare, Boss }
public enum RepositionReason
{
    TooFar,
    Stuck,
    OffNavMesh,
    MapChanged
}
