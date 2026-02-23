using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalFireballProjectile : MonoBehaviour
{
    private Transform target;
    private float speed;
    
    // 장판 관련 데이터
    private GameObject trailPrefab;
    private float trailDamage;
    private float trailDuration;
    private float spawnDistanceThreshold;
    private GameObject lastFireBoom;
    private float lastFireBoomDamage;

    private Vector3 lastSpawnPosition; // 마지막으로 장판을 깐 위치
    private Vector3 beforeDirection ;
    //private bool hasHit;
    private DoDamage damageComponent;

    // 무기에서 호출하여 데이터 초기화
    public void Setup(Transform newTarget, float newSpeed, GameObject trail, float tDamage, float tDuration, float tSpawnDist , 
                        GameObject FireBoomPrefab , float FireBoomDamage)
    {
        target = newTarget;
        speed = newSpeed;
        trailPrefab = trail;
        trailDamage = tDamage;
        trailDuration = tDuration;
        lastFireBoom = FireBoomPrefab;
        spawnDistanceThreshold = tSpawnDist;
        lastFireBoomDamage = FireBoomDamage;

        lastSpawnPosition = transform.position; // 시작점 초기화

        // 타겟 방향 보정
        if (target != null)
        {
            RotateTowardsTarget(target.position);
        }
    }
    private void OnEnable()
    {
        beforeDirection = transform.forward;
        damageComponent = GetComponent<DoDamage>();        
    }

    void Update()
    {
        // 1. 이동 로직
        Vector3 moveDirection = beforeDirection; // 기본 직진

        if (target != null)
        {
            // 타겟이 있으면 유도 (원하면 TurretBullet처럼 직사로 바꿔도 됨)
            beforeDirection = (target.position - transform.position).normalized;
            moveDirection = beforeDirection;
            RotateTowardsTarget(target.position);
        }

        transform.position += moveDirection * speed * Time.deltaTime;

        // 2. 장판 생성 로직 (거리 기준)
        // 마지막 스폰 위치로부터 일정 거리 이상 멀어졌는지 체크
        if (Vector3.Distance(transform.position, lastSpawnPosition) >= spawnDistanceThreshold)
        {
            SpawnTrail();
            lastSpawnPosition = transform.position; // 기준점 갱신
        }
    }

    void SpawnTrail()
    {
        // 오브젝트 풀에서 장판 가져오기
        GameObject trail = ObjectPoolingManager.instance.spawnGameObject(trailPrefab, transform.position, Quaternion.identity);

        // 장판의 DoDamage 스크립트 설정
        if (trail.TryGetComponent<DoDamage>(out var doDamage))
        {
            doDamage.damage = trailDamage;           // 계산된 장판 데미지 주입
            doDamage.lifeTime = trailDuration;       // 지속 시간 주입
            // *중요*: DoDamage가 풀링될 때 내부 타이머들이 리셋되도록 구현되어 있어야 합니다. 
            // 만약 DoDamage의 Start()에서만 waitTime 초기화가 일어난다면, OnEnable() 등에서 초기화하도록 수정이 필요할 수 있습니다.
            // 님이 주신 코드 기준으로는 waitTime이 Start에서 0이 되므로, 첫 타격은 바로 들어가고 그 다음부터 쿨타임이 돕니다.
        }
    }
    
    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // 충돌 및 파괴는 같이 붙어있는 DoDamage 스크립트가 처리합니다.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageComponent != null && damageComponent.TryCheakEnemy(collision))
        {
            GameObject FireBoom = Instantiate(lastFireBoom, transform.position, Quaternion.identity);
            if (FireBoom.TryGetComponent<DoDamage>(out var DamageComponent))
            {
                DamageComponent.damage = lastFireBoomDamage;//GetDamage();
            }
            damageComponent.TryApplyDamage(collision);
            //HandleSelfDestruction();
        }
    }
}
