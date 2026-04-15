using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ChargerEnemy : Enemy
{
    public enum ChargeType { Rush, Teleport }

    [Header("Charge Settings")]
    public ChargeType chargeType;
    public float chargeRange = 5.0f;      // 돌진을 시작할 거리
    public float chargePrepTime = 1.0f;   // 돌진 준비 시간 (위험 표시 시간)
    public float chargeSpeed = 20.0f;     // 실제 돌진 속도 (Rush 타입용)
    public float chargeDuration = 0.5f;   // 돌진 지속 시간 (Rush 타입용)
    public float recoveryTime = 0.5f;     // 돌진 후 멍때리는 시간

    [Header("Visuals")]
    public LineRenderer chargeIndicator;  // 돌진 경로 표시용
    public Color warningColor = Color.red;

    private bool isChargingAction = false; // 현재 돌진 패턴 중인가?
    private Vector3 chargeTargetPos;       // 돌진 목표 지점
    void Start()
    {
        if (chargeIndicator == null)
        {
            chargeIndicator = GetComponent<LineRenderer>();

            if (chargeIndicator == null)
            {
                chargeIndicator = GetComponentInChildren<LineRenderer>();
            }
        }

        if (chargeIndicator != null)
        {
            chargeIndicator.enabled = false;
            chargeIndicator.sortingOrder = 10; 
            chargeIndicator.useWorldSpace = true;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (chargeIndicator != null)
        {
            chargeIndicator.enabled = false;
            chargeIndicator.sortingOrder = 10; 
            chargeIndicator.useWorldSpace = true;
        }
        isChargingAction = false;
        stopMoving = false;
    }

    public override void ManualUpdate()
    {
        if (isChargingAction && !useSwarmMovement) 
        {
            // 돌진 중에는 쿨타임 감소 등 기본 로직만 수행하거나, 아예 독자적으로 돕니다.
            return; 
        }

        base.ManualUpdate(); 
    }

    protected override void DetermineState(float distanceSqrToPlayer)
    {
        if (isChargingAction) return;


        if (distanceSqrToPlayer <= chargeRange * chargeRange && coolDownTimer <= 0)
        {
            StartCoroutine(ChargeRoutine());
        }
        else
        {
            base.DetermineState(distanceSqrToPlayer);
        }
    }

    private IEnumerator ChargeRoutine()
    {
        isChargingAction = true;
        useSwarmMovement = false;
        stopMoving = true; // 이동 정지
        
        if (player != null)
        {
            // 플레이어 방향으로 돌진 목표 설정
            Vector3 dir = (player.position - transform.position).normalized;
            float distance = (chargeType == ChargeType.Rush) ? (chargeSpeed * chargeDuration) : chargeRange;
            chargeTargetPos = transform.position + (dir * distance);
            float enemyRadius = 0.5f; 
            float safeOffset = 0.1f;
            
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, enemyRadius, dir, distance, LayerMask.GetMask("Wall"));
            if (hit.collider != null)
            {
                chargeTargetPos = hit.point - ((Vector2)dir * (enemyRadius + safeOffset));
            }

            if (chargeIndicator != null)
            {
                chargeIndicator.enabled = true;
                chargeIndicator.positionCount = 2;
                chargeIndicator.SetPosition(0, transform.position);
                chargeIndicator.SetPosition(1, chargeTargetPos);

                chargeIndicator.startColor = warningColor;
                chargeIndicator.endColor = new Color(warningColor.r, warningColor.g, warningColor.b, 0);
            }
        }

        yield return new WaitForSeconds(chargePrepTime);

        if (chargeIndicator != null) chargeIndicator.enabled = false;


        if (chargeType == ChargeType.Teleport)
        {
            DamageAlongPath(transform.position, chargeTargetPos);

            transform.position = chargeTargetPos;
            /*
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled) agent.Warp(chargeTargetPos);
            */
            //CheckAreaDamage(transform.position, 2.0f);
        }
        else if (chargeType == ChargeType.Rush)
        {
            float t = 0;
            Vector3 startPos = transform.position;
            
            while (t < 1.0f)
            {
                t += Time.deltaTime / chargeDuration;
                transform.position = Vector3.Lerp(startPos, chargeTargetPos, t);
                
                CheckAreaDamage(transform.position, 1.0f);
                
                yield return null;
            }
        }

        yield return new WaitForSeconds(recoveryTime);

        
        isChargingAction = false;
        stopMoving = false;
        // coolDownTimer = coolDown; 
        useSwarmMovement = true;
    }

    private void DamageAlongPath(Vector3 start, Vector3 end)
    {
        Vector2 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        Collider2D col = GetComponent<Collider2D>();
        float radius = (col != null) ? Mathf.Min(col.bounds.extents.x, col.bounds.extents.y) : 0.5f;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(start, radius, direction, distance);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                IDamageable playerHealth = hit.collider.GetComponent<IDamageable>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }

    // 돌진 중 닿은 적에게 데미지 주기
    private void CheckAreaDamage(Vector3 center, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                IDamageable playerHealth = hit.GetComponent<IDamageable>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }

    // Enemy의 추상 메서드 구현 (돌진 몬스터는 별도의 평타가 없을 수 있음)
    public override void Attack()
    {
        // 돌진 자체가 공격이므로 비워두거나, 
        // 돌진 쿨타임일 때 근접 평타를 넣을 수도 있음
    }
}