
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FirstBossEnemy : Enemy
{
    public enum BossState
    {
        Idle,
        Chasing,
        MeleeAttack,
        RangedAttack,
        SpecialAttack
    }

    [Header("Boss Settings")]
    public BossState currentState = BossState.Idle;
    public float idleTime = 2f;
    public float rangedAttackRange = 10f;
    public float specialAttackCooldown = 15f;
    [Header("Slam Attack Settings")]
    public float windUpTime = 0.8f; // Time before the slam occurs
    public float attackOffset = 2f; // Distance in front of the enemy for the attack
    public Vector2 slamBoxSize = new Vector2(5f, 3f);
    public GameObject slamEffectPrefab; // Visual effect for the slam
    public LayerMask playerLayer; // To detect the player

    [Header("Slam Warning Settings")]
    public GameObject SquareSlamWarningPrefab; // '차오르는' 경고 이펙트 프리팹
    public GameObject CircleSlamWarningPrefab;
    public Color warningStartColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // 반투명 회색
    public Color warningEndColor = new Color(0, 0, 0, 0.9f);         // 진한 검은색
    public float JumpRadius = 3f; // The radius of the Jump attack
    public float landingKnockbackForce = 300f;


    private float specialAttackTimer;
    private DoDamage doDamage;
    private Coroutine currentCoroutine;
    private Collider2D[] bossCollider;

    void Start()
    {
        doDamage = GetComponent<DoDamage>();
        bossCollider = GetComponentsInChildren<Collider2D>();

        CantBeKnocked = true;

        ChangeState(BossState.Idle);
    }

    protected override void Update()
    {
        base.Update(); // Call the base class Update method to handle basic enemy logic

        if (player == null || currentNormalState == EnemyState.Dead) return;

        specialAttackTimer -= Time.deltaTime;

        // State transition logic
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (currentState == BossState.Idle || currentState == BossState.Chasing)
        {
            if (specialAttackTimer <= 0)
            {
                ChangeState(BossState.SpecialAttack);
            }
            else if (distanceToPlayer <= attackRange)
            {
                ChangeState(BossState.MeleeAttack);
            }
            else if (distanceToPlayer <= rangedAttackRange && currentState != BossState.RangedAttack)
            {
                ChangeState(BossState.RangedAttack);
            }
            else if (distanceToPlayer > rangedAttackRange && currentState != BossState.Chasing)
            {
                ChangeState(BossState.Chasing);
            }
        }
    }

    void ChangeState(BossState newState)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentState = newState;

        switch (currentState)
        {
            case BossState.Idle:
                currentCoroutine = StartCoroutine(Idle_State());
                break;
            case BossState.Chasing:
                currentCoroutine = StartCoroutine(Chasing_State());
                break;
            case BossState.MeleeAttack:
                currentCoroutine = StartCoroutine(MeleeAttack_State());
                break;
            case BossState.RangedAttack:
                currentCoroutine = StartCoroutine(RangedAttack_State());
                break;
            case BossState.SpecialAttack:
                currentCoroutine = StartCoroutine(SpecialAttack_State());
                break;
        }
    }

    IEnumerator Idle_State()
    {
        Debug.Log("Entering Idle State");
        stopMoving = true;
        yield return new WaitForSeconds(idleTime);
        stopMoving = false;
        ChangeState(BossState.Chasing); // Default to chasing after idling
    }

    IEnumerator Chasing_State()
    {
        Debug.Log("Entering Chasing State");
        stopMoving = false;
        // The base class's Update method handles the movement, so this coroutine can be simple
        // It will keep chasing until the Update method decides to change the state.
        yield return null;
    }

    IEnumerator MeleeAttack_State()
    {
        // Debug.Log("Entering Melee Attack State");
        // stopMoving = true;
        // Attack();
        // yield return new WaitForSeconds(coolDown);
        // stopMoving = false;
        // ChangeState(BossState.Idle);

        stopMoving = true;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 attackCenter = (Vector2)transform.position + directionToPlayer * attackOffset;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject indicatorInstance = null;
        if (SquareSlamWarningPrefab != null)
        {
            indicatorInstance = Instantiate(SquareSlamWarningPrefab, attackCenter, rotation);
        }

        Image warningImage = indicatorInstance?.GetComponentInChildren<Image>();
        if (warningImage != null) warningImage.color = warningStartColor;

        float timer = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = new Vector3(slamBoxSize.x, slamBoxSize.y, 1f);

        while (timer < windUpTime)
        {
            float progress = timer / windUpTime;
            if (indicatorInstance != null)
            {
                indicatorInstance.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            }
            if (warningImage != null)
            {
                warningImage.color = Color.Lerp(warningStartColor, warningEndColor, progress);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (indicatorInstance != null)
        {
            indicatorInstance.transform.localScale = endScale;
            if (warningImage != null) warningImage.color = warningEndColor;
        }

        if (slamEffectPrefab != null)
        {
            Instantiate(slamEffectPrefab, attackCenter, rotation);
        }

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackCenter, slamBoxSize, angle, playerLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
        }

        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }

        yield return new WaitForSeconds(coolDown);

        stopMoving = false;
        ChangeState(BossState.Idle);
    }

    IEnumerator RangedAttack_State()
    {
        Debug.Log("Entering Ranged Attack State");
        stopMoving = true;
        Debug.Log("Boss performs a ranged attack!");
        // Implement your ranged attack logic here
        yield return new WaitForSeconds(coolDown);
        stopMoving = false;
        ChangeState(BossState.Idle);
    }

    IEnumerator SpecialAttack_State()// 점프 공격
    {
        Debug.Log("Entering Special Attack State");
        stopMoving = true;

        //점프 모션 무적효과도 넣어줘야함. + 근접 데미지도 일시 중지
        spriteRenderer.enabled = false; 
        doDamage.damagePlayer = false;
        I_frame = true;

        foreach (var col in bossCollider) 
        {
            if (col != null) col.enabled = false;
        }
        if (agent != null) agent.enabled = false;

        Debug.Log("Boss is preparing a special attack!");
        yield return new WaitForSeconds(0.5f); // Wind-up time

        GameObject indicatorInstance = null;
        Image warningImage = null;

        transform.position = player.position;

        if (CircleSlamWarningPrefab != null)
        {
            indicatorInstance = Instantiate(CircleSlamWarningPrefab, transform.position, Quaternion.identity);
            warningImage = indicatorInstance.GetComponentInChildren<Image>();
            if (warningImage != null)
            {
                warningImage.color = warningStartColor;
            }
        }

        float timer = 0f;
        Vector3 startScale = Vector3.zero;
        float diameter = JumpRadius * 2f;
        Vector3 endScale = new Vector3(diameter, diameter, 1f);

        while (timer < windUpTime)
        {
            float progress = timer / windUpTime;
            if (indicatorInstance != null)
            {
                indicatorInstance.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            }
            if (warningImage != null)
            {
                warningImage.color = Color.Lerp(warningStartColor, warningEndColor, progress);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (indicatorInstance != null)
        {
            indicatorInstance.transform.localScale = endScale;
            if (warningImage != null)
            {
                warningImage.color = warningEndColor;
            }
        }
        foreach (var col in bossCollider) 
        {
            if (col != null) col.enabled = true;
        }

        if (agent != null) 
        {
            agent.enabled = true;
            agent.Warp(transform.position); 
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
        //모습 다시 로드 + 착지모션 추가예정.
        spriteRenderer.enabled = true;
        doDamage.damagePlayer = true;
        I_frame = false;

        if (slamEffectPrefab != null)
        {
            Instantiate(slamEffectPrefab, transform.position, Quaternion.identity);
        }

        HashSet<GameObject> hitTargets = new HashSet<GameObject>();
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, JumpRadius, playerLayer);

        foreach (var hitCollider in hitColliders)
        {
            GameObject targetObj = hitCollider.gameObject;
            if (hitTargets.Contains(targetObj)) 
            {
                continue; // 이미 때린 대상이면 아래 로직을 무시하고 다음 콜라이더로 넘어감!
            }
        
            // 때리지 않은 대상이라면 리스트에 추가
            hitTargets.Add(targetObj);

            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            Player_Controller playerScript = hitCollider.GetComponent<Player_Controller>();
            if (playerScript != null)
            {
                Vector2 knockbackDir = (hitCollider.transform.position - transform.position).normalized;
                
                // 방향, 힘(20f~), 제어 불능 시간(0.3초) 전달!
                playerScript.ApplyKnockback(knockbackDir, landingKnockbackForce, 0.3f);

                Debug.Log($"플레이어를 {knockbackDir} 방향으로 튕겨냅니다!");
            }
        }

        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }
        Debug.Log("Boss unleashes a powerful special attack!");
        // Implement your special attack logic here

        yield return new WaitForSeconds(coolDown);
        stopMoving = false;
        specialAttackTimer = specialAttackCooldown;
        ChangeState(BossState.Idle);
    }

    public override void Attack()
    {
        // This method is called for the melee attack
        Debug.Log(gameObject.name + " performs a melee attack, dealing " + damage + " damage.");
        // You can add an animation trigger here, similar to MeleeEnemy.cs
    }
}
