using UnityEngine;

// [핵심 설명] FireTrailOrbWeapon을 상속받아 부모가 가진 장판 데이터 처리 로직을 재사용합니다.
public class TurretFireTrailOrbWeapon : FireTrailOrbWeapon
{
    private GameObject projectilePrefab;
    private float fireRate;
    private float fireRange;
    private float projectileDamageScaling;
    private LayerMask enemyLayerMask;

    public override void Initialize(WeaponDataSO data)
    {
        // 1. 부모(FireTrailOrbWeapon -> OrbWeapon)의 초기화를 순차적으로 실행
        base.Initialize(data);

        // 2. 터렛 전용 데이터 로드
        if (data is TurretFireTrailOrbDataSO turretData)
        {
            this.projectilePrefab = turretData.projectilePrefab;
            this.fireRate = turretData.fireRate;
            this.fireRange = turretData.fireRange;
            this.projectileDamageScaling = turretData.projectileDamageScaling;
            this.enemyLayerMask = turretData.enemyLayerMask;
        }

        // 초기 생성된 오브들에게 터렛 정보 업데이트
        UpdateTurretInfoForExistingOrbs();
    }

    // [핵심 설명] 오브가 생성될 때 부모(장판, 기본 셋업) 로직을 먼저 부른 후, 터렛 정보를 추가 주입합니다.
    protected override void SetupSpawnedOrb(GameObject orb)
    {
        base.SetupSpawnedOrb(orb);

        if (orb.TryGetComponent<TurretFireTrailOrb>(out var turretOrbScript))
        {
            float finalProjectileDamage = GetDamage() * projectileDamageScaling;
            turretOrbScript.SetTurretInfo(projectilePrefab, fireRate, fireRange, finalProjectileDamage, enemyLayerMask);
        }
    }

    // 레벨업이나 데미지 변경 시 호출하여 실시간 반영
    private void UpdateTurretInfoForExistingOrbs()
    {
        float finalProjectileDamage = GetDamage() * projectileDamageScaling;
        foreach (var orbObj in spawnedOrbs)
        {
            if (orbObj != null && orbObj.TryGetComponent<TurretFireTrailOrb>(out var turretOrbScript))
            {
                turretOrbScript.SetTurretInfo(projectilePrefab, fireRate, fireRange, finalProjectileDamage, enemyLayerMask);
            }
        }
    }

    public override void UpgradeDamage(float amount)
    {
        base.UpgradeDamage(amount);
        UpdateTurretInfoForExistingOrbs();
    }
}