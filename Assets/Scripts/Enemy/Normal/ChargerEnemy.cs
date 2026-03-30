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
    public float recoveryTime = 1.5f;     // 돌진 후 멍때리는 시간

    [Header("Visuals")]
    public LineRenderer chargeIndicator;  // 돌진 경로 표시용 (LineRenderer 컴포넌트 필요)
    public Color warningColor = Color.red;

    private bool isChargingAction = false; // 현재 돌진 패턴 중인가?
    private Vector3 chargeTargetPos;       // 돌진 목표 지점
    void Start()
    {
        if (chargeIndicator == null)
        {
            // 1. 내 몸에 붙어있는지 확인
            chargeIndicator = GetComponent<LineRenderer>();

            // 2. 없다면 자식 오브젝트들 중에서 확인 (보통 이펙트는 자식으로 둡니다)
            if (chargeIndicator == null)
            {
                chargeIndicator = GetComponentInChildren<LineRenderer>();
            }
        }

        // 찾았으면 초기엔 꺼둠
        if (chargeIndicator != null)
        {
            chargeIndicator.enabled = false;
            // 2D 게임에서 라인이 잘 보이게 설정 (선택사항)
            chargeIndicator.sortingOrder = 10; 
            chargeIndicator.useWorldSpace = true; // 이동 시 라인이 따라오지 않고 고정되게 하려면 true 추천
        }
    }
    public override void OnEnable()
    {
        base.OnEnable();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (chargeIndicator != null)
        {
            chargeIndicator.enabled = false;
            // 2D 게임에서 라인이 잘 보이게 설정 (선택사항)
            chargeIndicator.sortingOrder = 10; 
            chargeIndicator.useWorldSpace = true; // 이동 시 라인이 따라오지 않고 고정되게 하려면 true 추천
        }
        isChargingAction = false;
        stopMoving = false;
    }

    protected override void Update()
    {
        // 부모의 기본 Update 로직 (피격, 넉백 등) 실행
        // 단, 돌진 중일 때는 부모의 이동 로직을 막아야 하므로 조건부 실행
        if (isChargingAction) 
        {
            // 돌진 중에는 쿨타임 감소 등 기본 로직만 수행하거나, 아예 독자적으로 돕니다.
            return; 
        }

        base.Update(); // 평소에는 NavMesh로 추적
    }

    // 부모의 DetermineState를 오버라이드하여 '돌진' 조건을 추가
    protected override void DetermineState(float distanceToPlayer)
    {
        base.DetermineState(distanceToPlayer);

        // 이미 돌진 중이면 상태 변경 안 함
        if (isChargingAction) return;

        // 공격 쿨타임이 찼고, 돌진 사정거리 안에 들어왔다면
        // (단, 너무 가까우면 그냥 평타를 때릴 수도 있으니 최소 거리 체크도 가능)
        if (distanceToPlayer <= chargeRange /* && coolDownTimer <= 0 */) // 쿨타임 변수 접근 권한 확인 필요
        {
            StartCoroutine(ChargeRoutine());
        }
    }

    private IEnumerator ChargeRoutine()
    {
        isChargingAction = true;
        stopMoving = true; // NavMesh 이동 정지
        
        // 1. [준비 단계] 목표 지점 설정 및 위험 표시
        if (player != null)
        {
            // 플레이어 방향으로 돌진 목표 설정 (플레이어 너머까지 돌진)
            Vector3 dir = (player.position - transform.position).normalized;
            float distance = (chargeType == ChargeType.Rush) ? (chargeSpeed * chargeDuration) : chargeRange;
            chargeTargetPos = transform.position + (dir * distance);
            
            // 벽 체크: 벽 너머로 돌진하지 않도록 Raycast
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, LayerMask.GetMask("Wall"));
            if (hit.collider != null)
            {
                chargeTargetPos = hit.point;
            }

            // 위험 표시 (LineRenderer) 켜기
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

        // 준비 시간 대기 (플레이어에게 피할 시간 줌)
        yield return new WaitForSeconds(chargePrepTime);

        // 위험 표시 끄기
        if (chargeIndicator != null) chargeIndicator.enabled = false;


        // 2. [돌진 단계] 타입에 따라 다르게 동작
        if (chargeType == ChargeType.Teleport)
        {
            // [텔레포트] 즉시 이동
            // 이펙트 펑! (Instantiate Effect)
            DamageAlongPath(transform.position, chargeTargetPos);

            transform.position = chargeTargetPos;
            
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled) agent.Warp(chargeTargetPos);
            
            //CheckAreaDamage(transform.position, 2.0f);
        }
        else if (chargeType == ChargeType.Rush)
        {
            // [실제 돌진] 빠르게 이동
            float t = 0;
            Vector3 startPos = transform.position;
            
            // 충돌 판정을 위해 Collider를 Trigger로 바꾸거나, 
            // OnTriggerEnter에서 데미지를 주도록 설정해야 함
            
            while (t < 1.0f)
            {
                t += Time.deltaTime / chargeDuration;
                transform.position = Vector3.Lerp(startPos, chargeTargetPos, t);
                
                // 돌진 중 충돌 체크 (간단하게 OverlapCircle로 구현)
                CheckAreaDamage(transform.position, 1.0f);
                
                yield return null;
            }
        }

        // 3. [회복 단계] 잠시 멈춤 (딜 타임)
        yield return new WaitForSeconds(recoveryTime);

        // 복귀
        isChargingAction = false;
        stopMoving = false;
        // 쿨타임 리셋 (부모 변수 접근 필요)
        // coolDownTimer = coolDown; 
    }

    private void DamageAlongPath(Vector3 start, Vector3 end)
    {
        Vector2 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        // 내 콜라이더 크기 가져오기 (반지름으로 사용)
        Collider2D col = GetComponent<Collider2D>();
        // 콜라이더가 없으면 기본값 0.5f, 있으면 너비의 절반 사용
        float radius = (col != null) ? Mathf.Min(col.bounds.extents.x, col.bounds.extents.y) : 0.5f;

        // CircleCastAll을 사용하여 경로상의 모든 충돌체 검사
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