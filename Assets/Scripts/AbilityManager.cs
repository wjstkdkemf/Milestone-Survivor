using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

  /*  public Dictionary<AbilityType, AbilityStats> abilityStats = new Dictionary<AbilityType, AbilityStats>();

    public AbilityUIManager abilityUIManager;
    public List<PowerUpScriptableObject> PowerUpScriptableObjects;
    public AbilityStatsScriptableObject Spark;
    public AbilityStatsScriptableObject IceOrb;
    public AbilityStatsScriptableObject SummonFirefox;
    public AbilityStatsScriptableObject Cyclone;
    public AbilityStatsScriptableObject PoisonCloud;

    private void Awake()
    {
        Instance = this;

    }

    // Initialize base stats for each ability from ScriptableObjects
    public void InitializeAbilityStats()
    {
        abilityStats.Clear();
        abilityStats[AbilityType.Spark] = new AbilityStats(
         baseDamage: Spark.BaseDamage,
         increaseDamagePercentage: Spark.IncreaseDamagePercentage,
         projectiles: Spark.ProjectileNumber,
         doubleDamage: Spark.ChanceDoubleDamage,
         elementDamage: Spark.IncreasedElementDamagePercentage,
         speed: Spark.ProjectileSpeed,
         pierces: Spark.ProjectilePierces,
         reducedCooldown: Spark.ReducedCooldown,
         increaseRange: Spark.IncreaseRange,
         effectDuration: Spark.EffectDuration
     );

        abilityStats[AbilityType.IceOrb] = new AbilityStats(
            baseDamage: IceOrb.BaseDamage,
            increaseDamagePercentage: IceOrb.IncreaseDamagePercentage,
            projectiles: IceOrb.ProjectileNumber,
            doubleDamage: IceOrb.ChanceDoubleDamage,
            elementDamage: IceOrb.IncreasedElementDamagePercentage,
            speed: IceOrb.ProjectileSpeed,
            pierces: IceOrb.ProjectilePierces,
            reducedCooldown: IceOrb.ReducedCooldown,
            increaseRange: IceOrb.IncreaseRange,
            effectDuration: IceOrb.EffectDuration
        );

        abilityStats[AbilityType.PoisonCloud] = new AbilityStats(
            baseDamage: PoisonCloud.BaseDamage,
            increaseDamagePercentage: PoisonCloud.IncreaseDamagePercentage,
            projectiles: PoisonCloud.ProjectileNumber,
            doubleDamage: PoisonCloud.ChanceDoubleDamage,
            elementDamage: PoisonCloud.IncreasedElementDamagePercentage,
            speed: PoisonCloud.ProjectileSpeed,
            pierces: PoisonCloud.ProjectilePierces,
            reducedCooldown: PoisonCloud.ReducedCooldown,
            increaseRange: PoisonCloud.IncreaseRange,
            effectDuration: PoisonCloud.EffectDuration
        );

        // Additional abilities
        abilityStats[AbilityType.SummonFirefox] = new AbilityStats(
            baseDamage: SummonFirefox.BaseDamage,
            increaseDamagePercentage: SummonFirefox.IncreaseDamagePercentage,
            projectiles: SummonFirefox.ProjectileNumber,
            doubleDamage: SummonFirefox.ChanceDoubleDamage,
            elementDamage: SummonFirefox.IncreasedElementDamagePercentage,
            speed: SummonFirefox.ProjectileSpeed,
            pierces: SummonFirefox.ProjectilePierces,
            reducedCooldown: SummonFirefox.ReducedCooldown,
            increaseRange: SummonFirefox.IncreaseRange,
            effectDuration: SummonFirefox.EffectDuration
        );

        abilityStats[AbilityType.Cyclone] = new AbilityStats(
            baseDamage: Cyclone.BaseDamage,
            increaseDamagePercentage: Cyclone.IncreaseDamagePercentage,
            projectiles: Cyclone.ProjectileNumber,
            doubleDamage: Cyclone.ChanceDoubleDamage,
            elementDamage: Cyclone.IncreasedElementDamagePercentage,
            speed: Cyclone.ProjectileSpeed,
            pierces: Cyclone.ProjectilePierces,
            reducedCooldown: Cyclone.ReducedCooldown,
            increaseRange: Cyclone.IncreaseRange,
            effectDuration: Cyclone.EffectDuration
        );

    }


    private void Start()
    {
        InitializeAbilityStats();
        Setinfo();
    }

    public void Setinfo()
    {
        foreach (PowerUpScriptableObject skill in PowerUpScriptableObjects)
        {
            if (skill.isUnlocked)
            {
                AbilityStats stats = GetAbilityStats(skill.abilityType);

                //   int currentUpgradeLevel = Mathf.Clamp(skill.CurrentUpgradeLevel, 0, skill.Upgrades.Count() - 1);
                int currentUpgradeLevel = skill.CurrentUpgradeLevel - 1;
                float currentValue;
                if (currentUpgradeLevel < 0)
                {
                    currentValue = 0;
                }
                else currentValue = skill.Upgrades[currentUpgradeLevel].value;
                UpgradeEffect currentEffect = skill.Upgrades[currentUpgradeLevel].effect;


                switch (currentEffect)
                {
                    case UpgradeEffect.BaseDamage:
                        stats._BaseDamage = currentValue;
                        break;
                    case UpgradeEffect.IncreasedDamagePercentage:
                        stats._IncreaseDamagePercentage = currentValue;
                        break;
                    case UpgradeEffect.ProjectileNumber:
                        stats._ProjectileNumber = (int)currentValue;
                        break;
                    case UpgradeEffect.ChanceDoubleDamage:
                        stats._ChanceDoubleDamage = currentValue;
                        break;
                    case UpgradeEffect.IncreasedElementDamage:
                        stats._IncreasedElementDamage = currentValue;
                        break;
                    case UpgradeEffect.ProjectileSpeed:
                        stats._ProjectileSpeed = currentValue;
                        break;
                    case UpgradeEffect.ProjectilePierces:
                        stats._ProjectilePierces = (int)currentValue;
                        break;

                    // Handle new cases for new effects
                    case UpgradeEffect.ReducedCooldown:
                        stats._ReducedCooldown = currentValue;
                        break;
                    case UpgradeEffect.IncreaseRange:
                        stats._IncreaseRange = currentValue;
                        break;
                    case UpgradeEffect.EffectDuration:
                        stats._EffectDuration = currentValue;
                        break;

                    default:
                        Debug.LogWarning($"Unknown upgrade effect: {currentEffect} for {skill.abilityType}");
                        break;
                }

                SetAbilityStats(skill.abilityType, stats);
            }
        }

        if (abilityUIManager != null)
        {
            // abilityUIManager.ShowAllAbilityStats();
        }
    }

    public AbilityStats GetAbilityStats(AbilityType abilityType)
    {
        if (abilityStats.ContainsKey(abilityType))
        {
            return abilityStats[abilityType];
        }
        else
        {
            Debug.LogWarning($"AbilityType {abilityType} not found. Creating default AbilityStats.");
            AbilityStats defaultStats = new AbilityStats(baseDamage: 0);  // Default stats with baseDamage 0, customize as needed
            abilityStats[abilityType] = defaultStats;
            return defaultStats;
        }
    }

    // Set stats for a specific ability
    public void SetAbilityStats(AbilityType abilityType, AbilityStats stats)
    {
        if (abilityStats.ContainsKey(abilityType))
            abilityStats[abilityType] = stats;
        else
            abilityStats.Add(abilityType, stats);

        if (abilityUIManager != null)
            abilityUIManager.ShowAllAbilityStats();
    }*/
}
[System.Serializable]
public class AbilityStats
{
    public float _BaseDamage;
    public float _IncreaseDamagePercentage;
    public int _ProjectileNumber;
    public float _ChanceDoubleDamage;
    public float _IncreasedElementDamage;
    public float _IncreaseBaseAbilityDamage;
    public float _ProjectileSpeed;
    public int _ProjectilePierces;

    // Add new fields for the new effects
    public float _ReducedCooldown;   // New Stat for Reduced Cooldown
    public float _IncreaseRange;     // New Stat for Range Increase
    public float _EffectDuration;    // New Stat for Effect Duration

    public AbilityStats(
        float baseDamage,
        float increaseDamagePercentage = 0,
        int projectiles = 1,
        float doubleDamage = 0,
        float elementDamage = 0,
        float speed = 1,
        int pierces = 0,
        float increaseBaseAbilityDamage = 0,
        float reducedCooldown = 0,      // New Parameter for Reduced Cooldown
        float increaseRange = 0,        // New Parameter for Range Increase
        float effectDuration = 0        // New Parameter for Effect Duration
    )
    {
        _BaseDamage = baseDamage;
        _IncreaseDamagePercentage = increaseDamagePercentage;
        _ProjectileNumber = projectiles;
        _ChanceDoubleDamage = doubleDamage;
        _IncreaseBaseAbilityDamage = increaseBaseAbilityDamage;
        _IncreasedElementDamage = elementDamage;
        _ProjectileSpeed = speed;
        _ProjectilePierces = pierces;

        // Initialize new stats
        _ReducedCooldown = reducedCooldown;
        _IncreaseRange = increaseRange;
        _EffectDuration = effectDuration;
    }
}