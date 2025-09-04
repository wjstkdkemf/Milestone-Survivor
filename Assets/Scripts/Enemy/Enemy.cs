using System.Collections;
using System.Collections.Generic;
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
    protected Transform player;
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
    public Material originalMaterial;
    protected Coroutine flashRoutine;
    public bool IsActived = false;
    public bool DontUseObjectPooling;
    public bool boss;
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
        player = GameObject.FindWithTag("Player").transform;
        if (boss)
        {
            spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); // Initialize the spriteRenderer
            originalMaterial = spriteRenderer.material;      // Initialize the original material
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); // Initialize the spriteRenderer
            originalMaterial = spriteRenderer.material;      // Initialize the original material
        }
       
    }
    private void OnEnable()
    {
        health = maxhealth;
        IsActived = true;
        spriteRenderer.material = originalMaterial;
        spriteRenderer.color = Color.white;
        GameManager.Instance.activeEnemies++;
    }
    private void OnDisable()
    {
        IsActived = false;
    }
    void Start()
    {
      
    }

    // Abstract method for attacking behavior
    public abstract void Attack();

    // Abstract method for death behavior
    public virtual void Die()
    {
        IsActived = false;
        // Handle enemy death
        GameManager.Instance.NumberOfKills++;
        GameManager.Instance.activeEnemies--;

        // Attempt to drop loot
        LootDrop lootDrop = GetComponent<LootDrop>();
        if (lootDrop != null)
        {
            lootDrop.DropLoot();
        }

        //Debug.Log(gameObject.name + " has died.");
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

    // Method to handle movement and logic for chasing, attacking, and fleeing
    protected virtual void Update()
    {
        if (player == null || stopMoving || IsActived == false) return;

        knockBackTime -= Time.deltaTime;
        Vector3 delta = player.position - transform.position;
        coolDownTimer -= Time.deltaTime;
        // Handle facing direction
        UpdateFacingDirection(delta);

        // Check distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Determine states (chasing, attacking, fleeing)
        DetermineState(distanceToPlayer);

        // Movement based on state
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

    // Determines whether the enemy should be chasing, attacking, or fleeing
    protected virtual void DetermineState(float distanceToPlayer)
    {
        if (distanceToPlayer <= range)
        {
            inAttackRange = false;
            chasing = true;
            running = false;
        }

        if (distanceToPlayer <= attackRange)
        {
            inAttackRange = true;
            if (canRun && distanceToPlayer <= escapeRange)
            {
                running = true;
            }
            chasing = false;
        }
        else
        {
            inAttackRange = false;
            running = false;
        }
    }

    // Handles movement logic based on state (chasing, running, knockback)
    protected virtual void HandleMovement(float distanceToPlayer, Vector3 delta)
    {
        if (knockBackTime > 0 && !CantBeKnocked)
        {
            // Apply knockback
            transform.position = Vector2.MoveTowards(transform.position, player.position, -1 * knockBackForce * Time.deltaTime);
        }
        else
        {
            // If chasing, move towards the player
            if (chasing)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            }
            // If running away, move in the opposite direction
            else if (running)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, -1 * speed * Time.deltaTime);
            }

            // If within attack range, perform the attack
            if (inAttackRange && coolDownTimer <= 0)
            {
                Attack();
                coolDownTimer = coolDown;

            }
        }
    }

    // Implementation of the TakeDamage method from IDamageable
    public virtual void TakeDamage(float amount, float knockBackDuration = .2f)
    {
        // Play hurt sound (if you have an audio manager)
        // AudioManager.instance.PlaySound("Enemy_Hurt");

        // Instantiate damage text
        if (DamageText != null)
        {
            GameObject text = Instantiate(DamageText, transform.position, Quaternion.identity);
            text.GetComponent<TMP_Text>().text = amount.ToString(); // Display the amount of damage taken
        }

        // Flash the sprite if flashMaterial is set
        if (flashMaterial != null && !boss&&IsActived)
            Flash();

        // Decrease health and debug log
        health -= amount;
        // Debug.Log($"{gameObject.name} took {amount} damage, current health: {health}");

        // Check if health is below zero
        if (health <= 0 && IsActived)
        {
            IsActived = false;
            Die(); // Handle death
        }

        knockBackTime = _knockBackDuration; // Apply knockback duration

    }

    // Check if the enemy is alive (IDamageable)
    public bool IsAlive()
    {
        return IsActived;
    }

    // Method for handling knockback logic
    protected virtual void ApplyKnockback(Vector3 direction, float force)
    {

    }
    public void Flash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine); // Stop any ongoing flash to avoid overlap
        }
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    public IEnumerator FlashRoutine()
    {
        if (IsActived == false)
            yield return null;
        // Save the original color of the sprite
        Color originalColor = spriteRenderer.color;

        // Change the sprite color to red (damage flash)
        spriteRenderer.color = Color.red;

        // Wait for the specified flash duration
        yield return new WaitForSeconds(duration);
        if (IsActived == false)
            yield return null;
        // Restore the original color after the flash
        spriteRenderer.color = originalColor;

        // Reset the flash routine to null
        flashRoutine = null;
    }

    // Method to get a random spawn position within a circle around the monster's death position
    private Vector2 GetRandomPositionAround(Vector2 centerPosition, float radius)
    {
        // Random angle in radians
        float angle = Random.Range(0f, Mathf.PI * 2);
        // Random distance within the radius
        float distance = Random.Range(0f, radius);
        // Calculate the position
        float x = centerPosition.x + Mathf.Cos(angle) * distance;
        float y = centerPosition.y + Mathf.Sin(angle) * distance;
        return new Vector2(x, y);
    }
}

[System.Serializable]
public enum EnemyRarity { Normal, Magic, Rare, Boss }
