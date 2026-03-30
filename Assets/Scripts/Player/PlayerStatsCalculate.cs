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

    // Level-up bonuses
    private float levelDamage = 0;
    private float levelHealthRegen = 0;
    private float levelCooldownReduction = 0;
    private float levelArmor = 0;
    private float levelMaxHealth = 0;


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

    public void SetBaseStats(float maxHealth, float speed, float healthRegen, float luck, float damage, List<StatModifier> statModifiers)
    {
        baseMaxHealth = maxHealth;
        baseSpeed = speed;
        baseHealthRegen = healthRegen;
        baseLuck = luck;
        baseDamage = damage;
        baseStatModifiers = new List<StatModifier>(statModifiers);// 복사
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

    public void AddPowerUpBonus(PowerUpType type, float value)
    {
        switch (type)
        {
            case PowerUpType.Damage: powerUpDamage += value; break;
            case PowerUpType.MovementSpeed: powerUpSpeed += value; break;
            case PowerUpType.HealthRegeneration: powerUpHealthRegen += value; break;
            case PowerUpType.XPBoost: powerUpExperienceBonus += value; break;
            case PowerUpType.CooldownReduction: powerUpCooldownReduction += value; break;
            case PowerUpType.projectilespeed: powerUpProjectileSpeed += value; break;
            case PowerUpType.luckBoost: powerUpLuck += value; break;
            case PowerUpType.KnockBack: powerUpKnockBack += value; break;
            case PowerUpType.Armor: powerUpArmor += value; break;
            case PowerUpType.DobleDamageChance: powerUpDoubleDamageChance += value; break;
            case PowerUpType.Encounter: powerUpEncounter += (int)value; break;
        }
        UpdatePlayerStats();
    }

    public void AddRealTimeBonus(PowerUpType type, float value)
    {
        switch (type)
        {
            case PowerUpType.Damage: realTimeDamage += value; break;
            case PowerUpType.MovementSpeed: realTimeSpeed += value; break;
            case PowerUpType.HealthRegeneration: realTimeHealthRegen += value; break;
            case PowerUpType.XPBoost: realTimeExperienceBonus += value; break;
            case PowerUpType.CooldownReduction: realTimeCooldownReduction += value; break;
            case PowerUpType.luckBoost: realTimeLuck += value; break;
            case PowerUpType.KnockBack: realTimeKnockBack += value; break;
            case PowerUpType.Armor: realTimeArmor += value; break;
            case PowerUpType.DobleDamageChance: realTimeDoubleDamageChance += value; break;
        }
        UpdatePlayerStats();
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

        realTimeDamage = 0;
        realTimeSpeed = 0;
        realTimeHealthRegen = 0;
        realTimeExperienceBonus = 0;
        realTimeCooldownReduction = 0;
        realTimeLuck = 0;
        realTimeKnockBack = 0;
        realTimeArmor = 0;
        realTimeDoubleDamageChance = 0;
        realTimeProjectileSpeed = 0;

        UpdatePlayerStats();
    }

    public void UpdatePlayerStats()
    {
        if (PlayerStats.Instance != null)
        {
            // EquipmentEffectManager에서 스탯 보너스 가져오기
            float equipDamage = EquipmentEffectManager.Instance.GetStatBonus("Damage");
            float equipSpeed = EquipmentEffectManager.Instance.GetStatBonus("MovementSpeed");
            float equipHealthRegen = EquipmentEffectManager.Instance.GetStatBonus("HealthRegeneration");
            float equipExpBonus = EquipmentEffectManager.Instance.GetStatBonus("XPBoost");
            float equipCooldown = EquipmentEffectManager.Instance.GetStatBonus("CooldownReduction");
            float equipLuck = EquipmentEffectManager.Instance.GetStatBonus("luckBoost");
            float equipKnockback = EquipmentEffectManager.Instance.GetStatBonus("KnockBack");
            float equipArmor = EquipmentEffectManager.Instance.GetStatBonus("Armor");
            float equipDoubleDamage = EquipmentEffectManager.Instance.GetStatBonus("DobleDamageChance");
            float equipMaxHealth = EquipmentEffectManager.Instance.GetStatBonus("MaxHealth");
            float equiDamageRation = EquipmentEffectManager.Instance.GetStatBonus("DamageRation");
            float equipProjectileSpeed = EquipmentEffectManager.Instance.GetStatBonus("ProjectileSpeed");

            int equipEncount = (int)EquipmentEffectManager.Instance.GetStatBonus("Encount");
            // 최종 스탯 계산
            PlayerStats.Instance.DamageBonus = (baseDamage + powerUpDamage + realTimeDamage + levelDamage) * (1 + equiDamageRation) + equipDamage;
            PlayerStats.Instance.SpeedBonus = baseSpeed + powerUpSpeed + realTimeSpeed + equipSpeed;
            PlayerStats.Instance.HealthRegeneration = baseHealthRegen + powerUpHealthRegen + realTimeHealthRegen + equipHealthRegen + levelHealthRegen;
            PlayerStats.Instance.experienceBonus = powerUpExperienceBonus + realTimeExperienceBonus + equipExpBonus ;
            PlayerStats.Instance.cooldownReduction = powerUpCooldownReduction + realTimeCooldownReduction + equipCooldown + levelCooldownReduction;
            PlayerStats.Instance.LuckBonus = baseLuck + powerUpLuck + realTimeLuck + equipLuck;
            PlayerStats.Instance.KnockBackBonus = baseKnockBack + powerUpKnockBack + realTimeKnockBack + equipKnockback;
            PlayerStats.Instance.ArmorBonus = baseArmor + powerUpArmor + realTimeArmor + equipArmor + levelArmor;
            PlayerStats.Instance.DoubleDamageChance = baseDoubleDamageChance + powerUpDoubleDamageChance + realTimeDoubleDamageChance + equipDoubleDamage;
            PlayerStats.Instance.projectileSpeedBonus = powerUpProjectileSpeed + equipProjectileSpeed + realTimeProjectileSpeed;
            if (GameObject.FindGameObjectWithTag("GameScene") != null && GameProgressManager.Instance.IsUnlocked("Tutorial"))
                EnCounterSystem.Instance.maxEncounter = EnCounterSystem.Instance.normalMaxEncounter + equipEncount + powerUpEncounter;

            if (PlayerStats.Instance.Player != null)
            {
                PlayerStats.Instance.Player.GetComponent<Player_Controller>().movmentSpeed = PlayerStats.Instance.SpeedBonus;
                PlayerStats.Instance.Player.GetComponent<PlayerHealth>().MaxHealth = baseMaxHealth + powerUpMaxHealth + realTimeMaxHealth + equipMaxHealth + levelMaxHealth;
            }
        }
    }
}
