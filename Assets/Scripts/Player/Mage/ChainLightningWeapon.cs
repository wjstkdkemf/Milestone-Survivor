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
            float finalDamage = GetDamage();
            chainSkill.Fire(target, finalDamage, currentProjectileSpeed, currentChainCount, currentChainRange, currentDamageReduction);
        }
    }
    
    public float GetDamage()
    {
        float bonus = (playerStats != null) ? playerStats.DamageBonus : 0;
        return currentBaseDamage + (bonus * currentScaling);
    }

    public override void LevelUp()
    {
        currentBaseDamage += 3f;
        if (currentChainCount < 5) 
            currentChainCount++;
        Debug.Log($"[Chain Lightning Level Up] 데미지: {currentBaseDamage}, 체인 횟수: {currentChainCount}");
    }
}