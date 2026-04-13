using UnityEngine;

// [핵심 설명] FireTrailOrbWeapon을 상속받아 부모가 가진 장판 데이터 처리 로직을 재사용합니다.
public class TurretFireTrailOrbWeapon : FireTrailOrbWeapon
{
    private GameObject projectilePrefab;
    private float fireRate;
    private float fireRange;
    private float projectileDamageScaling;
    private float projectileSpeed;

    public override void Initialize(WeaponDataSO data)
    {
        base.Initialize(data);

        if (data is TurretFireTrailOrbDataSO turretData)
        {
            projectilePrefab = turretData.projectilePrefab;
            fireRate = turretData.fireRate;
            fireRange = turretData.fireRange;
            projectileDamageScaling = turretData.projectileDamageScaling;
            projectileSpeed = turretData.projectileSpeed;
        }

        UpdateTurretInfoForExistingOrbs();
    }

    protected override void SetupSpawnedOrb(GameObject orb)
    {
        base.SetupSpawnedOrb(orb);

        if (orb.TryGetComponent<TurretFireTrailOrb>(out var turretOrbScript))
        {
            float finalProjectileDamage = GetDamage() * projectileDamageScaling;
            turretOrbScript.SetTurretInfo(projectilePrefab, fireRate, fireRange, finalProjectileDamage, projectileSpeed);
        }
    }

    private void UpdateTurretInfoForExistingOrbs()
    {
        float finalProjectileDamage = GetDamage() * projectileDamageScaling;
        foreach (var orbObj in spawnedOrbs)
        {
            if (orbObj != null && orbObj.TryGetComponent<TurretFireTrailOrb>(out var turretOrbScript))
            {
                turretOrbScript.SetTurretInfo(projectilePrefab, fireRate, fireRange, finalProjectileDamage , projectileSpeed);
            }
        }
    }

    public override void UpgradeDamage(float amount)
    {
        base.UpgradeDamage(amount);
        UpdateTurretInfoForExistingOrbs();
    }
}