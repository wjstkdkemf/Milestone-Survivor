using UnityEngine;

// 날아가는 투사체에 부착 (Rigidbody2D, Collider2D, DoDamage와 함께 사용)
public class EvolvedFireballProjectile : MonoBehaviour
{
    private Transform target;
    private float speed;
    
    // 장판 관련 데이터
    private GameObject trailPrefab;
    private float trailDamage;
    private float trailDuration;
    private float spawnDistanceThreshold;

    private Vector3 lastSpawnPosition; // 마지막으로 장판을 깐 위치
    //private bool hasHit;
    private DoDamage damageComponent;

    // 무기에서 호출하여 데이터 초기화
    public void Setup(Transform newTarget, float newSpeed, GameObject trail, float tDamage, float tDuration, float tSpawnDist)
    {
        target = newTarget;
        speed = newSpeed;
        trailPrefab = trail;
        trailDamage = tDamage;
        trailDuration = tDuration;
        spawnDistanceThreshold = tSpawnDist;

        lastSpawnPosition = transform.position; // 시작점 초기화

        // 타겟 방향 보정
        if (target != null)
        {
            RotateTowardsTarget(target.position);
        }
    }
    private void OnEnable()
    {
        damageComponent = GetComponent<DoDamage>();        
    }

    void Update()
    {
        Vector3 moveDirection = transform.right; // 기본 직진

        if (target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
            RotateTowardsTarget(target.position);
        }

        transform.position += moveDirection * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, lastSpawnPosition) >= spawnDistanceThreshold)
        {
            SpawnTrail();
            lastSpawnPosition = transform.position; // 기준점 갱신
        }
    }

    void SpawnTrail()
    {
        GameObject trail = ObjectPoolingManager.Instance.spawnGameObject(trailPrefab, transform.position, Quaternion.identity);

        if (trail == null) return;

        if (trail.TryGetComponent<DoDamage>(out var doDamage))
        {
            doDamage.damage = trailDamage;           // 계산된 장판 데미지 주입
            doDamage.lifeTime = trailDuration;       // 지속 시간 주입
        }
    }
    
    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}