using UnityEngine;

public class EvolvedFireballProjectile : SkillProjectileBase
{
    private Transform target;
    private float speed;
    
    // 장판 관련 데이터
    private GameObject trailPrefab;
    private float trailDamage;
    private float trailDuration;
    private float spawnDistanceThreshold;

    private Vector3 lastSpawnPosition; // 마지막으로 장판을 깐 위치
    public void SetupEvo(Transform newTarget, float newSpeed, GameObject trail, long directDmg, float tDamage, float tDuration, float tSpawnDist , string weaponID)
    {
        target = newTarget;
        speed = newSpeed;
        damage = directDmg;
        hitRadius = 0.5f; // 알바생(Job)이 쓸 투사체 직격 크기
        maxHits = 1;      // 관통 불가

        this.WeaponID = weaponID;

        trailPrefab = trail;
        trailDamage = tDamage;
        trailDuration = tDuration;
        spawnDistanceThreshold = tSpawnDist;

        lastSpawnPosition = transform.position;

        if (target != null)
        {
            RotateTowardsTarget(target.position);
        }
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
            lastSpawnPosition = transform.position; 
        }
    }

    void SpawnTrail()
    {
        if (trailPrefab == null) return;

        GameObject trail = ObjectPoolingManager.Instance.spawnGameObject(trailPrefab, transform.position, Quaternion.identity);

        if (trail != null && trail.TryGetComponent<AuraZone>(out var trailSkill))
        {
            trailSkill.SetupAura(0.5f, (long)trailDamage, 1.5f, false, 0f, WeaponID , trailDuration); 
        }
    }
    
    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    public override void OnHit(Enemy hitEnemy)
    {
    }
}