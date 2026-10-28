using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsCalculate : MonoBehaviour
{
    public static PlayerStatsCalculate Instance { get; private set; }

    // Base stats
    public float baseMaxHealth = 1;
    public float baseDamage = 0;
    public float baseSpeed = 0;
    public float baseHealthRegen = 0;
    public float baseLuck = 0;
    public float baseKnockBack = 0;
    public float baseArmor = 0;
    public float baseCooldownReduction = 0;
    public float baseDoubleDamageChance = 0;
    public List<StatModifier> baseStatModifiers;
    public int Level = 0;

    // Power-up bonuses
    private float powerUpDamage = 0;
    private float powerUpSpeed = 0;
    private float powerUpHealthRegen = 0;
    private float powerUpExperienceBonus = 0;
    private float powerUpCooldownReduction = 0;
    private float powerUpLuck = 0;
    private float powerUpKnockBack = 0;
    private float powerUpArmor = 0;
    private float powerUpDoubleDamageChance = 0;
    private float powerUpMaxHealth = 0;
    private float powerUpProjectileSpeed = 0;
    private int powerUpEncounter = 0;

    private float powerUpPercentageDamage = 0;
    private float powerUpPercentageSpeed = 0;
    private float powerUpPercentageHealthRegen = 0;
    private float powerUpPercentageExperienceBonus = 0;
    private float powerUpPercentageCooldownReduction = 0;
    private float powerUpPercentageLuck = 0;
    private float powerUpPercentageKnockBack = 0;
    private float powerUpPercentageArmor = 0;
    private float powerUpPercentageDoubleDamageChance = 0;
    private float powerUpPercentageMaxHealth = 0;
    private float powerUpPercentageProjectileSpeed = 0;

    // In-game real-time bonuses
    private float realTimeDamage = 0;
    private float realTimeSpeed = 0;
    private float realTimeHealthRegen = 0;
    private float realTimeExperienceBonus = 0;
    private float realTimeCooldownReduction = 0;
    private float realTimeLuck = 0;
    private float realTimeKnockBack = 0;
    private float realTimeArmor = 0;
    private float realTimeDoubleDamageChance = 0;
    private float realTimeMaxHealth = 0;
    private float realTimeProjectileSpeed = 0;

    private float realTimePercentageDamage = 0;
    private float realTimePercentageMaxHealth = 0;
    private float realTimePercentageSpeed = 0;
    private float realTimePercentageHealthRegen = 0;
    private float realTimePercentageExperienceBonus = 0;
    private float realTimePercentageCooldownReduction = 0;

    // Level-up bonuses
    private float levelDamage = 0;
    private float levelHealthRegen = 0;
    private float levelCooldownReduction = 0;
    private float levelArmor = 0;
    private float levelMaxHealth = 0;

    public event System.Action AttackChanged;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetBaseStats(float maxHealth,float Armor ,float speed, float healthRegen, float luck, float damage, List<StatModifier> statModifiers, float cooldownReduction = 0f)
    {
        baseMaxHealth = maxHealth;
        baseSpeed = speed;
        baseArmor = Armor;
        baseHealthRegen = healthRegen;
        baseLuck = luck;
        baseDamage = damage;
        baseCooldownReduction = cooldownReduction;
        baseStatModifiers = statModifiers != null
            ? new List<StatModifier>(statModifiers)
            : new List<StatModifier>();
        UpdatePlayerStats();
    }
    public void LevelUpBonus(int Level)
    {
        this.Level = Level;

        // Reset level bonuses
        levelDamage = 0;
        levelHealthRegen = 0;
        levelArmor = 0;
        levelMaxHealth = 0;

        float LevelRation = Level * 0.2f;
        if (baseStatModifiers != null)
        {
            foreach (var modifier in baseStatModifiers)
            {
                switch (modifier.statName) // Assuming StatModifier has a 'stat' string field
                {
                    case "Damage":
                        levelDamage += modifier.value * LevelRation;
                        break;
                    case "HealthRegeneration":
                        levelHealthRegen += modifier.value * LevelRation;
                        break;
                    case "Armor":
                        levelArmor += modifier.value * LevelRation;
                        break;
                    case "MaxHealth":
                        levelMaxHealth += modifier.value * LevelRation;
                        break;
                }
            }
        }

        UpdatePlayerStats();
    }

    public void AddPowerUpBonus(PowerUpType type, float value, BonusMode mode)
    {
        if(mode == BonusMode.Flat)
        {
            switch (type)
            {
                case PowerUpType.Damage: powerUpDamage += value; break;
                case PowerUpType.MovementSpeed: powerUpSpeed += value; break;
                case PowerUpType.HealthRegeneration: powerUpHealthRegen += value; break;
                case PowerUpType.MaxHealth: powerUpMaxHealth += value; break;
                case PowerUpType.XPBoost: powerUpExperienceBonus += value; break;
                case PowerUpType.CooldownReduction: powerUpCooldownReduction += value; break;
                case PowerUpType.projectilespeed: powerUpProjectileSpeed += value; break;
                case PowerUpType.luckBoost: powerUpLuck += value; break;
                case PowerUpType.KnockBack: powerUpKnockBack += value; break;
                case PowerUpType.Armor: powerUpArmor += value; break;
                case PowerUpType.DobleDamageChance: powerUpDoubleDamageChance += value; break;
                case PowerUpType.Encounter: powerUpEncounter += (int)value; break;
            }
        }
        else if(mode == BonusMode.Percent)
        {
            switch (type)
            {
                case PowerUpType.Damage: powerUpPercentageDamage += value; break;
                case PowerUpType.MovementSpeed: powerUpPercentageSpeed += value; break;
                case PowerUpType.HealthRegeneration: powerUpPercentageHealthRegen += value; break;
                case PowerUpType.MaxHealth: powerUpPercentageMaxHealth += value; break;
                case PowerUpType.XPBoost: powerUpPercentageExperienceBonus += value; break;
                case PowerUpType.CooldownReduction: powerUpPercentageCooldownReduction += value; break;
                case PowerUpType.projectilespeed: powerUpPercentageProjectileSpeed += value; break;
                case PowerUpType.luckBoost: powerUpPercentageLuck += value; break;
                case PowerUpType.KnockBack: powerUpPercentageKnockBack += value; break;
                case PowerUpType.Armor: powerUpPercentageArmor += value; break;
                case PowerUpType.DobleDamageChance: powerUpPercentageDoubleDamageChance += value; break;
            }
        }
        UpdatePlayerStats();
    }

    public void AddRealTimeBonus(UpgradeScriptableObject upgrade)
    {
        if (upgrade == null) return;

        UpgradeScriptableObject.UpgradeType type = upgrade.upgradeType;
        float value = upgrade.statValue;
        if (upgrade.bonusMode == BonusMode.Flat)
        {
            switch (type)
            {
                case UpgradeScriptableObject.UpgradeType.Stat_Might: realTimeDamage += value; break;
                case UpgradeScriptableObject.UpgradeType.Stat_MaxHealth: realTimeMaxHealth += value; break;
                case UpgradeScriptableObject.UpgradeType.Stat_MoveSpeed: realTimeSpeed += value; break;
                case UpgradeScriptableObject.UpgradeType.Stat_Heal: realTimeHealthRegen += value; break;
                case UpgradeScriptableObject.UpgradeType.Stat_Growth: realTimeExperienceBonus += value; break;
                case UpgradeScriptableObject.UpgradeType.Stat_Cooldown: realTimeCooldownReduction += value; break;
            }
        }
        else if (upgrade.bonusMode == BonusMode.Percent)
        {
            switch (type)
            {
                case UpgradeScriptableObject.UpgradeType.Stat_Might: realTimePercentageDamage += value; break;
                case UpgradeScriptableObject.UpgradeType.Stat_MaxHealth: realTimePercentageMaxHealth += value; break;
                case UpgradeScriptableObject.UpgradeType.Stat_MoveSpeed: realTimePercentageSpeed += value; break;
                case UpgradeScriptableObject.UpgradeType.Stat_Heal: realTimePercentageHealthRegen += value; break;
                case UpgradeScriptableObject.UpgradeType.Stat_Growth: realTimePercentageExperienceBonus += value; break;
                case UpgradeScriptableObject.UpgradeType.Stat_Cooldown: realTimePercentageCooldownReduction += value; break;
            }
        }
        UpdatePlayerStats();
    }

    public bool TryGetRealTimeBonus(
        UpgradeScriptableObject.UpgradeType type,
        BonusMode mode,
        out float value)
    {
        value = 0f;
        if (mode != BonusMode.Flat && mode != BonusMode.Percent)
            return false;

        bool isPercent = mode == BonusMode.Percent;
        switch (type)
        {
            case UpgradeScriptableObject.UpgradeType.Stat_Might:
                value = isPercent ? realTimePercentageDamage : realTimeDamage;
                return true;
            case UpgradeScriptableObject.UpgradeType.Stat_MaxHealth:
                value = isPercent ? realTimePercentageMaxHealth : realTimeMaxHealth;
                return true;
            case UpgradeScriptableObject.UpgradeType.Stat_MoveSpeed:
                value = isPercent ? realTimePercentageSpeed : realTimeSpeed;
                return true;
            case UpgradeScriptableObject.UpgradeType.Stat_Heal:
                value = isPercent ? realTimePercentageHealthRegen : realTimeHealthRegen;
                return true;
            case UpgradeScriptableObject.UpgradeType.Stat_Growth:
                value = isPercent ? realTimePercentageExperienceBonus : realTimeExperienceBonus;
                return true;
            case UpgradeScriptableObject.UpgradeType.Stat_Cooldown:
                value = isPercent ? realTimePercentageCooldownReduction : realTimeCooldownReduction;
                return true;
            default:
                return false;
        }
    }

    public void ResetBonuses()
    {
        powerUpDamage = 0;
        powerUpSpeed = 0;
        powerUpHealthRegen = 0;
        powerUpExperienceBonus = 0;
        powerUpCooldownReduction = 0;
        powerUpLuck = 0;
        powerUpKnockBack = 0;
        powerUpArmor = 0;
        powerUpDoubleDamageChance = 0;
        powerUpMaxHealth = 0;
        powerUpProjectileSpeed = 0;
        powerUpEncounter = 0;

        powerUpPercentageDamage = 0;
        powerUpPercentageSpeed = 0;
        powerUpPercentageHealthRegen = 0;
        powerUpPercentageExperienceBonus = 0;
        powerUpPercentageCooldownReduction = 0;
        powerUpPercentageLuck = 0;
        powerUpPercentageKnockBack = 0;
        powerUpPercentageArmor = 0;
        powerUpPercentageDoubleDamageChance = 0;
        powerUpPercentageMaxHealth = 0;
        powerUpPercentageProjectileSpeed = 0;

        realTimeDamage = 0;
        realTimeSpeed = 0;
        realTimeHealthRegen = 0;
        realTimeExperienceBonus = 0;
        realTimeCooldownReduction = 0;
        realTimeLuck = 0;
        realTimeKnockBack = 0;
        realTimeArmor = 0;
        realTimeDoubleDamageChance = 0;
        realTimeMaxHealth = 0;
        realTimeProjectileSpeed = 0;

        realTimePercentageDamage = 0;
        realTimePercentageMaxHealth = 0;
        realTimePercentageSpeed = 0;
        realTimePercentageHealthRegen = 0;
        realTimePercentageExperienceBonus = 0;
        realTimePercentageCooldownReduction = 0;

        levelDamage = 0;
        levelHealthRegen = 0;
        levelCooldownReduction = 0;
        levelArmor = 0;
        levelMaxHealth = 0;

        UpdatePlayerStats();
    }

    public void UpdatePlayerStats()
    {
        if (PlayerStats.Instance == null)
            return;

        EquipmentEffectManager equipmentEffectManager = EquipmentEffectManager.Instance;
        long prevDamage = PlayerStats.Instance.DamageBonus;

        // EquipmentEffectManager에서 스탯 보너스 가져오기
        float equipDamage = GetEquipmentBonus(equipmentEffectManager, "Damage");
        float equipSpeed = GetEquipmentBonus(equipmentEffectManager, "MovementSpeed");
        float equipHealthRegen = GetEquipmentBonus(equipmentEffectManager, "HealthRegeneration");
        float equipExpBonus = GetEquipmentBonus(equipmentEffectManager, "XPBoost");
        float equipCooldown = GetEquipmentBonus(equipmentEffectManager, "CooldownReduction");
        float equipLuck = GetEquipmentBonus(equipmentEffectManager, "luckBoost");
        float equipKnockback = GetEquipmentBonus(equipmentEffectManager, "KnockBack");
        float equipArmor = GetEquipmentBonus(equipmentEffectManager, "Armor");
        float equipDoubleDamage = GetEquipmentBonus(equipmentEffectManager, "DobleDamageChance");
        float equipMaxHealth = GetEquipmentBonus(equipmentEffectManager, "MaxHealth");
        float equiDamageRation = GetEquipmentBonus(equipmentEffectManager, "DamageRation");
        float equipProjectileSpeed = GetEquipmentBonus(equipmentEffectManager, "ProjectileSpeed");

        int equipEncount = (int)GetEquipmentBonus(equipmentEffectManager, "Encount");
        // 최종 스탯 계산
        // Percent values use display units: 10 means a 10% increase.
        // Damage percentages stack additively; flat equipment damage stays outside the multiplier.
        PlayerStats.Instance.DamageBonus = (long)((baseDamage + powerUpDamage + realTimeDamage + levelDamage)
            * (1f + equiDamageRation + (powerUpPercentageDamage + realTimePercentageDamage) * 0.01f) + equipDamage);
        PlayerStats.Instance.SpeedBonus = (baseSpeed + powerUpSpeed + realTimeSpeed + equipSpeed)
            * (1f + (powerUpPercentageSpeed + realTimePercentageSpeed) * 0.01f);
        PlayerStats.Instance.HealthRegeneration = (baseHealthRegen + powerUpHealthRegen + realTimeHealthRegen + equipHealthRegen + levelHealthRegen)
            * (1f + (powerUpPercentageHealthRegen + realTimePercentageHealthRegen) * 0.01f);
        PlayerStats.Instance.experienceBonus = (powerUpExperienceBonus + realTimeExperienceBonus + equipExpBonus)
            * (1f + (powerUpPercentageExperienceBonus + realTimePercentageExperienceBonus) * 0.01f);
        PlayerStats.Instance.cooldownReduction = (baseCooldownReduction + powerUpCooldownReduction + realTimeCooldownReduction + equipCooldown + levelCooldownReduction)
            * (1f + (powerUpPercentageCooldownReduction + realTimePercentageCooldownReduction) * 0.01f);
        PlayerStats.Instance.LuckBonus = (baseLuck + powerUpLuck + realTimeLuck + equipLuck)
            * (1f + powerUpPercentageLuck * 0.01f);
        PlayerStats.Instance.KnockBackBonus = (baseKnockBack + powerUpKnockBack + realTimeKnockBack + equipKnockback)
            * (1f + powerUpPercentageKnockBack * 0.01f);
        PlayerStats.Instance.ArmorBonus = (baseArmor + powerUpArmor + realTimeArmor + equipArmor + levelArmor)
            * (1f + powerUpPercentageArmor * 0.01f);
        PlayerStats.Instance.DoubleDamageChance = (baseDoubleDamageChance + powerUpDoubleDamageChance + realTimeDoubleDamageChance + equipDoubleDamage)
            * (1f + powerUpPercentageDoubleDamageChance * 0.01f);
        PlayerStats.Instance.projectileSpeedBonus = (powerUpProjectileSpeed + equipProjectileSpeed + realTimeProjectileSpeed)
            * (1f + powerUpPercentageProjectileSpeed * 0.01f);

        if (GameObject.FindGameObjectWithTag("GameScene") != null &&
            GameProgressManager.Instance != null &&
            GameProgressManager.Instance.IsUnlocked("Tutorial") &&
            EnCounterSystem.Instance != null)
        {
            EnCounterSystem.Instance.maxEncounter = EnCounterSystem.Instance.normalMaxEncounter + equipEncount + powerUpEncounter;
        }
        if (prevDamage != PlayerStats.Instance.DamageBonus)
            AttackChanged?.Invoke();
        if (PlayerStats.Instance.Player != null)
        {
            Player_Controller playerController = PlayerStats.Instance.Player.GetComponent<Player_Controller>();
            if (playerController != null)
                playerController.movmentSpeed = PlayerStats.Instance.SpeedBonus;

            PlayerHealth playerHealth = PlayerStats.Instance.Player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.MaxHealth = (baseMaxHealth + powerUpMaxHealth + realTimeMaxHealth + equipMaxHealth + levelMaxHealth)
                    * (1f + (powerUpPercentageMaxHealth + realTimePercentageMaxHealth) * 0.01f);
        }
    }

    private float GetEquipmentBonus(EquipmentEffectManager equipmentEffectManager, string statName)
    {
        return equipmentEffectManager != null ? equipmentEffectManager.GetStatBonus(statName) : 0f;
    }
}
