using UnityEngine;

// [핵심 설명] FireTrailOrb를 상속받아 장판 생성과 닿았을 때의 지속 딜 기능을 그대로 유지합니다.
public class TurretFireTrailOrb : FireTrailOrb
{
    private GameObject projectilePrefab;
    private float fireRate;
    private float fireRange;
    private float projectileDamage;
    private LayerMask enemyLayerMask;

    private float fireTimer;

    // 무기 관리자에서 호출하여 터렛 관련 정보를 주입
    public void SetTurretInfo(GameObject prefab, float rate, float range, float pDamage, LayerMask layer)
    {
        this.projectilePrefab = prefab;
        this.fireRate = rate;
        this.fireRange = range;
        this.projectileDamage = pDamage;
        this.enemyLayerMask = layer;

        fireTimer = fireRate; // 생성 직후 바로 쏘게 하려면 0으로 설정해도 좋습니다.
    }

    // [핵심 설명] 부모의 Update(장판 생성, 타격 연산)를 실행한 뒤, 미사일 발사 타이머를 돌립니다.
    /*protected override void Update()
    {
        base.Update(); // 중요: 부모의 장판 쿨타임과 ZoneDamage 로직이 여기서 실행됨

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            TryFireProjectile();
            fireTimer = fireRate;
        }
    }*/

    private void TryFireProjectile()
    {
        // 1. 오브 기준으로 가장 가까운 적 탐색
        Transform target = FindClosestEnemy();
        if (target == null) return;

        // 2. 미사일 발사 (오브젝트 풀링 사용)
        GameObject bullet = ObjectPoolingManager.Instance.spawnGameObject(projectilePrefab, transform.position, Quaternion.identity);

        // 3. 미사일 설정 (TurretBullet 스크립트에 타겟 주입)
        if (bullet.TryGetComponent<TurretBullet>(out var turretBullet))
        {
            //turretBullet.EnemyPosition = target;
        }

        // 4. 미사일 데미지 설정 (DoDamage 스크립트)
        if (bullet.TryGetComponent<DoDamage>(out var damageComponent))
        {
            damageComponent.damage = this.projectileDamage;
        }
    }

    // 터렛 스킬에서 썼던 로직을 오브 중심으로 실행합니다.
    private Transform FindClosestEnemy()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, fireRange, enemyLayerMask);
        Transform closest = null;
        float closestDistSqr = Mathf.Infinity;

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent<IDamageable>(out _))
            {
                float distSqr = (hit.transform.position - transform.position).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closest = hit.transform;
                }
            }
        }
        return closest;
    }
}