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
        if (fireTimer <= 0f && Time.timeScale > 0f)
            TryFireProjectile();
    }
    private void TryFireProjectile()
    {
        Enemy target = EnemySwarmSystem.Instance.GetClosestEnemy(transform.position, fireRange);
        if (target == null)
        {
            fireTimer = fireRate;
            return;
        }

        var pool = ObjectPoolingManager.Instance;
        if (pool == null) return;

        GameObject bullet = pool.spawnGameObject(projectilePrefab, transform.position, Quaternion.identity);
        if (bullet == null) return;
        if (!bullet.TryGetComponent<TurretBullet>(out var bulletSkill))
        {
            pool.ReturnObjectToPool(bullet);
            return;
        }

        bulletSkill.damage = (long)projectileDamage;
        bulletSkill.Fire(target.transform.position, projectileSpeed , WeaponID);
        fireTimer = fireRate;
    }
}
