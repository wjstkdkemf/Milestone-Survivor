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
    private float currentBaseDamage;
    private float currentScaling;

    private GameObject projectilePrefab;
    private float cooldownTimer;
    private PlayerStats playerStats;

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
            currentBaseDamage = lightningData.baseDamage;
            currentScaling = lightningData.playerDamageScaling;

            attackMotion = lightningData.attackMotion;

            this.WeaponID = lightningData.WeaponId;

            projectilePrefab = lightningData.projectilePrefab;
        }
        else
        {
            Debug.LogError("잘못된 데이터! ChainLightningDataSO가 필요합니다.");
        }

        if (PlayerStats.Instance != null)
             playerStats = PlayerStats.Instance;
        else
             playerStats = GetComponentInParent<PlayerStats>();

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

        if (firstTarget != null)
        {
            FireFirstLightning(firstTarget);
            cooldownTimer = currentCooldownTime;
        }
    }

    private void FireFirstLightning(Enemy target)
    {
        GameObject lightningObj = ObjectPoolingManager.Instance.spawnGameObject(
            projectilePrefab, transform.position, Quaternion.identity
        );

        if (lightningObj != null && lightningObj.TryGetComponent<ChainLightningProjectile>(out var chainSkill))
        {
            long finalDamage = GetDamage();
            chainSkill.Fire(target, finalDamage, currentProjectileSpeed, currentChainCount, currentChainRange, currentDamageReduction , WeaponID);
        }
    }
    
    public long GetDamage()
    {
        float bonus = (playerStats != null) ? playerStats.DamageBonus : 0;
        float before = currentBaseDamage + (bonus * currentScaling);
        return (long)before;
    }

    public override void LevelUp()
    {
        currentBaseDamage += 3f;
        if (currentChainCount < 5) 
            currentChainCount++;
        Debug.Log($"[Chain Lightning Level Up] 데미지: {currentBaseDamage}, 체인 횟수: {currentChainCount}");
    }

    public override UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = base.GetUpgradePreview(upgrade);

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.damage",
            currentBaseDamage.ToString("0.##"),
            (currentBaseDamage + 3f).ToString("0.##")
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.chain_count",
            currentChainCount.ToString(),
            Mathf.Min(currentChainCount + 1, 5).ToString()
        ));

        return preview;
    }
}
