using UnityEngine;

public class PlayerStatsCalculate : MonoBehaviour
{
    public static PlayerStatsCalculate Instance { get; private set; }

    // Base stats
    public float baseMaxHealth = 1;
    public float baseDamage = 0;
    public float baseSpeed = 0;
    public float baseHealthRegen = 0;
    public float baseExperienceBonus = 0;
    public float baseProjectileSpeed = 0;
    public float baseCooldownReduction = 0;
    public float baseLuck = 0;
    public float baseKnockBack = 0;
    public float baseArmor = 0;
    public float baseDoubleDamageChance = 0;

    // Power-up bonuses
    private float powerUpDamage = 0;
    private float powerUpSpeed = 0;
    private float powerUpHealthRegen = 0;
    private float powerUpExperienceBonus = 0;
    private float powerUpProjectileSpeed = 0;
    private float powerUpCooldownReduction = 0;
    private float powerUpLuck = 0;
    private float powerUpKnockBack = 0;
    private float powerUpArmor = 0;
    private float powerUpDoubleDamageChance = 0;
    private float powerUpMaxHealth = 0;

    // In-game real-time bonuses
    private float realTimeDamage = 0;
    private float realTimeSpeed = 0;
    private float realTimeHealthRegen = 0;
    private float realTimeExperienceBonus = 0;
    private float realTimeProjectileSpeed = 0;
    private float realTimeCooldownReduction = 0;
    private float realTimeLuck = 0;
    private float realTimeKnockBack = 0;
    private float realTimeArmor = 0;
    private float realTimeDoubleDamageChance = 0;
    private float realTimeMaxHealth = 0;

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

    public void SetBaseStats(float maxHealth, float speed, float healthRegen, float experienceBonus, float luck, float damage)
    {
        baseMaxHealth = maxHealth;
        baseSpeed = speed;
        baseHealthRegen = healthRegen;
        baseExperienceBonus = experienceBonus;
        baseLuck = luck;
        baseDamage = damage;
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
            case PowerUpType.ProjectileSpeed: powerUpProjectileSpeed += value; break;
            case PowerUpType.CooldownReduction: powerUpCooldownReduction += value; break;
            case PowerUpType.luckBoost: powerUpLuck += value; break;
            case PowerUpType.KnockBack: powerUpKnockBack += value; break;
            case PowerUpType.Armor: powerUpArmor += value; break;
            case PowerUpType.DobleDamageChance: powerUpDoubleDamageChance += value; break;
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
            case PowerUpType.ProjectileSpeed: realTimeProjectileSpeed += value; break;
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
        powerUpProjectileSpeed = 0;
        powerUpCooldownReduction = 0;
        powerUpLuck = 0;
        powerUpKnockBack = 0;
        powerUpArmor = 0;
        powerUpDoubleDamageChance = 0;

        realTimeDamage = 0;
        realTimeSpeed = 0;
        realTimeHealthRegen = 0;
        realTimeExperienceBonus = 0;
        realTimeProjectileSpeed = 0;
        realTimeCooldownReduction = 0;
        realTimeLuck = 0;
        realTimeKnockBack = 0;
        realTimeArmor = 0;
        realTimeDoubleDamageChance = 0;
        
        UpdatePlayerStats();
    }

    private void UpdatePlayerStats()
    {
        if (PlayerStats.Instance != null)
        {
            // EquipmentEffectManager에서 스탯 보너스 가져오기
            float equipDamage = EquipmentEffectManager.Instance.GetStatBonus("Damage");
            float equipSpeed = EquipmentEffectManager.Instance.GetStatBonus("MovementSpeed");
            float equipHealthRegen = EquipmentEffectManager.Instance.GetStatBonus("HealthRegeneration");
            float equipExpBonus = EquipmentEffectManager.Instance.GetStatBonus("XPBoost");
            float equipProjectileSpeed = EquipmentEffectManager.Instance.GetStatBonus("ProjectileSpeed");
            float equipCooldown = EquipmentEffectManager.Instance.GetStatBonus("CooldownReduction");
            float equipLuck = EquipmentEffectManager.Instance.GetStatBonus("luckBoost");
            float equipKnockback = EquipmentEffectManager.Instance.GetStatBonus("KnockBack");
            float equipArmor = EquipmentEffectManager.Instance.GetStatBonus("Armor");
            float equipDoubleDamage = EquipmentEffectManager.Instance.GetStatBonus("DobleDamageChance");
            float equipMaxHealth = EquipmentEffectManager.Instance.GetStatBonus("MaxHealth");
            float equiDamageRation = EquipmentEffectManager.Instance.GetStatBonus("DamageRation");

            // 최종 스탯 계산
            PlayerStats.Instance.DamageBonus = (baseDamage + powerUpDamage + realTimeDamage + equipDamage) * (1 + equiDamageRation);
            PlayerStats.Instance.SpeedBonus = baseSpeed + powerUpSpeed + realTimeSpeed + equipSpeed;
            PlayerStats.Instance.HealthRegeneration = baseHealthRegen + powerUpHealthRegen + realTimeHealthRegen + equipHealthRegen;
            PlayerStats.Instance.experienceBonus = baseExperienceBonus + powerUpExperienceBonus + realTimeExperienceBonus + equipExpBonus;
            PlayerStats.Instance.projectileSpeedBonus = baseProjectileSpeed + powerUpProjectileSpeed + realTimeProjectileSpeed + equipProjectileSpeed;
            PlayerStats.Instance.cooldownReduction = baseCooldownReduction + powerUpCooldownReduction + realTimeCooldownReduction + equipCooldown;
            PlayerStats.Instance.LuckBonus = baseLuck + powerUpLuck + realTimeLuck + equipLuck;
            PlayerStats.Instance.KnockBackBonus = baseKnockBack + powerUpKnockBack + realTimeKnockBack + equipKnockback;
            PlayerStats.Instance.ArmorBonus = baseArmor + powerUpArmor + realTimeArmor + equipArmor;
            PlayerStats.Instance.DoubleDamageChance = baseDoubleDamageChance + powerUpDoubleDamageChance + realTimeDoubleDamageChance + equipDoubleDamage;

            if (PlayerStats.Instance.Player != null)
            {
                PlayerStats.Instance.Player.GetComponent<Player_Controller>().movmentSpeed = baseSpeed + powerUpSpeed + realTimeSpeed + equipSpeed;
                PlayerStats.Instance.Player.GetComponent<PlayerHealth>().MaxHealth = baseMaxHealth + powerUpMaxHealth + realTimeMaxHealth + equipMaxHealth;
                Debug.Log(baseMaxHealth + "체력 셋팅성공");
            }
        }
    }
}
