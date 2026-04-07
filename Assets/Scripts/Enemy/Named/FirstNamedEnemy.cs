using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class FirstNamedEnemy : NavEnemy
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
    public float slamRadius = 3f; // The radius of the slam attack
    public float windUpTime = 0.8f; // Time before the slam occurs
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

    public override void ManualUpdate()
    {
        base.ManualUpdate(); // Call the base class Update method to handle basic enemy logic

        if (player == null) return;

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

        GameObject indicatorInstance = null;
        Image warningImage = null;

        if (slamWarningPrefab != null)
        {
            indicatorInstance = Instantiate(slamWarningPrefab, transform.position, Quaternion.identity);
            warningImage = indicatorInstance.GetComponentInChildren<Image>();
            if (warningImage != null)
            {
                warningImage.color = warningStartColor;
            }
        }

        float timer = 0f;
        Vector3 startScale = Vector3.zero;
        float diameter = slamRadius * 2f;
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

        if (slamEffectPrefab != null)
        {
            Instantiate(slamEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, slamRadius, playerLayer);
        foreach (var hitCollider in hitColliders)
        {
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
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
        ChangeState(State.Idle);
    }

    public override void Attack()
    {
        // This method is now effectively replaced by the SlamAttack_State coroutine.
        // We leave it empty to satisfy the abstract class requirement, 
        // or you could have it log a warning if it's ever called unexpectedly.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}