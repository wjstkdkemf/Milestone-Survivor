using UnityEngine;

public class TurretFireTrailOrb : FireTrailOrb
{
    private GameObject projectilePrefab;
    private float fireRate;
    private float fireRange;
    private float projectileDamage;
    private float projectileSpeed;

    private float fireTimer;

    // 무기 관리자에서 호출하여 터렛 관련 정보를 주입
    public void SetTurretInfo(GameObject prefab, float rate, float range, float pDamage, float pSpeed , string WeaponID)
    {
        projectilePrefab = prefab;
        fireRate = rate;
        fireRange = range;
        projectileDamage = pDamage;
        projectileSpeed = pSpeed;

        this.WeaponID = WeaponID;

        fireTimer = 0f;
    }

    protected override void Update()
    {
        base.Update(); 

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            TryFireProjectile();
            fireTimer = fireRate;
        }
    }
    private void TryFireProjectile()
    {
        Enemy target = EnemySwarmSystem.Instance.GetClosestEnemy(transform.position, fireRange);
        if (target == null) return;

        GameObject bullet = ObjectPoolingManager.Instance.spawnGameObject(projectilePrefab, transform.position, Quaternion.identity);

        if (bullet != null && bullet.TryGetComponent<TurretBullet>(out var bulletSkill))
        {
            // 투사체의 방향 계산
            Vector3 direction = (target.transform.position - transform.position).normalized;
            
            // TODO: 사장님의 투사체 스크립트에 맞게 초기화 함수를 호출하세요.
            // 예: bulletSkill.Fire(direction, projectileDamage, projectileSpeed);
            bulletSkill.Fire(target.transform.position, projectileSpeed , WeaponID);
            bulletSkill.damage = (long)projectileDamage;
            /*
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            */
        }
    }
}