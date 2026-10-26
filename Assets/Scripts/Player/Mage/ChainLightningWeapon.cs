using System.Collections.Generic;
using UnityEngine;

public class ChainLightningWeapon : WeaponBase
{
    // [런타임 상태 변수]
    private float currentCooldownTime;
    private float currentInitialRange;
    private int currentChainCount;
    private float currentChainRange;
    private float currentDamageReduction;
    private float currentScaling;

    private GameObject projectilePrefab;
    private float cooldownTimer;

    public override void Initialize(WeaponDataSO data)
    {
        if (data is ChainLightningDataSO lightningData)
        {
            currentCooldownTime = lightningData.baseCooldown;
            currentInitialRange = lightningData.initialRange;
            currentProjectileSpeed = lightningData.projectileSpeed;
            currentChainCount = lightningData.chainCount;
            currentChainRange = lightningData.chainRange;
            currentDamageReduction = lightningData.damageReductionPerBounce;
            currentDamage = lightningData.baseDamage;
            currentScaling = lightningData.playerDamageScaling;

            attackMotion = lightningData.attackMotion;

            this.WeaponID = lightningData.WeaponId;

            projectilePrefab = lightningData.projectilePrefab;
        }
        else
        {
            Debug.LogError("잘못된 데이터! ChainLightningDataSO가 필요합니다.");
        }

        cooldownTimer = 0f; 
    }

    public override void OnUpdate()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (cooldownTimer <= 0f)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        Enemy firstTarget = EnemySwarmSystem.Instance.GetClosestEnemy(transform.position, currentInitialRange);

        if (firstTarget != null && FireFirstLightning(firstTarget))
        {
            cooldownTimer = GetCooldown(currentCooldownTime);
        }
    }

    private bool FireFirstLightning(Enemy target)
    {
        var pool = ObjectPoolingManager.Instance;
        if (pool == null) return false;

        GameObject lightningObj = pool.spawnGameObject(
            projectilePrefab, transform.position, Quaternion.identity
        );

        if (lightningObj == null) return false;
        if (!lightningObj.TryGetComponent<ChainLightningProjectile>(out var chainSkill))
        {
            pool.ReturnObjectToPool(lightningObj);
            return false;
        }

        long finalDamage = GetDamage();
        chainSkill.Fire(target, finalDamage, currentProjectileSpeed, currentChainCount, currentChainRange, currentDamageReduction , WeaponID);
        return true;
    }
    
    public long GetDamage()
    {
        return CalculateDamage(currentDamage, currentScaling);
    }

    protected override void RecalculateLevelStats()
    {
        if (!(myData is ChainLightningDataSO data)) return;
        int upgrades = CurrentLevel - 1;
        currentDamage = data.baseDamage + 3L * upgrades;
        currentChainCount = Mathf.Min(data.chainCount + upgrades, Mathf.Max(5, data.chainCount));
        DevLog.Log($"[Chain Lightning Level Up] 데미지: {currentDamage}, 체인 횟수: {currentChainCount}");
    }

    public override UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = base.GetUpgradePreview(upgrade);

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.damage",
            currentDamage.ToString(),
            (currentDamage + 3L).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.chain_count",
            currentChainCount.ToString(),
            Mathf.Min(currentChainCount + 1, 5).ToString()
        ));

        return preview;
    }
}
