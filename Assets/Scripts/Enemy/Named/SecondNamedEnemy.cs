using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SecondNamedEnemy : Enemy
{
    public enum State
    {
        Idle,
        Chasing,
        SlamAttack
    }

    [Header("State Machine")]
    public State currentState = State.Idle;

    [Header("Slam Attack Settings")]
    public float windUpTime = 0.8f; // Time before the slam occurs
    public float attackOffset = 2f; // Distance in front of the enemy for the attack
    public Vector2 slamBoxSize = new Vector2(5f, 3f);
    public GameObject slamEffectPrefab; // Visual effect for the slam
    public LayerMask playerLayer; // To detect the player

    [Header("Slam Warning Settings")]
    public GameObject slamWarningPrefab; // '차오르는' 경고 이펙트 프리팹
    public Color warningStartColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // 반투명 회색
    public Color warningEndColor = new Color(0, 0, 0, 0.9f);         // 진한 검은색

    private Coroutine currentCoroutine;

    void Start()
    {
        ChangeState(State.Idle);
    }

    protected override void Update()
    {
        base.Update(); // Call the base class Update method to handle basic enemy logic

        if (player == null || currentNormalState == EnemyState.Dead) return;

        // State transition logic
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (currentState == State.Idle || currentState == State.Chasing)
        {
            if (distanceToPlayer <= attackRange)
            {
                ChangeState(State.SlamAttack);
            }
            else if (distanceToPlayer > attackRange && currentState != State.Chasing)
            {
                ChangeState(State.Chasing);
            }
        }
    }

    void ChangeState(State newState)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentState = newState;

        switch (currentState)
        {
            case State.Idle:
                currentCoroutine = StartCoroutine(Idle_State());
                break;
            case State.Chasing:
                currentCoroutine = StartCoroutine(Chasing_State());
                break;
            case State.SlamAttack:
                currentCoroutine = StartCoroutine(SlamAttack_State());
                break;
        }
    }

    IEnumerator Idle_State()
    {
        stopMoving = true;
        yield return new WaitForSeconds(1f); // Idle for 1 second
        stopMoving = false;
        ChangeState(State.Chasing);
    }

    IEnumerator Chasing_State()
    {
        stopMoving = false;
        // Base class handles movement
        yield return null;
    }

    IEnumerator SlamAttack_State()
    {
        stopMoving = true;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 attackCenter = (Vector2)transform.position + directionToPlayer * attackOffset;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject indicatorInstance = null;
        if (slamWarningPrefab != null)
        {
            indicatorInstance = Instantiate(slamWarningPrefab, attackCenter, rotation);
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

        //yield return new WaitForSeconds(coolDown);

        stopMoving = false;
        ChangeState(State.Idle);
    }

    public override void Attack()
    {
        // This method is now effectively replaced by the SlamAttack_State coroutine.
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.red;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 attackCenter = (Vector2)transform.position + directionToPlayer * attackOffset;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(attackCenter, rotation, Vector3.one);

        Gizmos.DrawWireCube(Vector3.zero, slamBoxSize);

        Gizmos.matrix = oldMatrix;
    }
}
