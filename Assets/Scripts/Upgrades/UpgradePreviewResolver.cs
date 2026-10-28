using System.Collections.Generic;
using UnityEngine;

public class UpgradePreviewResolver : MonoBehaviour
{
    [SerializeField] private PlayerWeaponController weaponManager;

    public UpgradePreviewData Resolve(UpgradeScriptableObject upgrade)
    {
        if (upgrade == null)
            return null;

        if (upgrade.IsWeaponUpgrade &&
            weaponManager != null &&
            weaponManager.TryGetWeapon(upgrade.linkedWeaponData, out WeaponBase weapon, includeEquipmentWeapons: true))
        {
            return weapon.GetUpgradePreview(upgrade);
        }

        return UpgradePreviewFactory.CreateFallbackPreview(upgrade);
    }
}

public static class UpgradePreviewFactory
{
    public static UpgradePreviewData CreateFallbackPreview(UpgradeScriptableObject upgrade)
    {
        if (upgrade == null)
            return null;

        if (upgrade.IsWeaponUpgrade)
        {
            return CreateNewWeaponPreview(upgrade);
        }

        return CreateStatPreview(upgrade);
    }

    private static UpgradePreviewData CreateNewWeaponPreview(UpgradeScriptableObject upgrade)
    {
        WeaponDataSO weaponData = upgrade.linkedWeaponData;
        UpgradePreviewData preview = CreateBasePreview(upgrade);

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.status",
            "upgrade.value.none",
            "upgrade.value.new",
            true,
            true
        ));
        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.level",
            "upgrade.value.unowned",
            "Lv.1",
            true
        ));

        AddInitialWeaponStats(preview, weaponData);

        return preview;
    }

    private static void AddInitialWeaponStats(UpgradePreviewData preview, WeaponDataSO data)
    {
        const int firstLevel = 1;

        // Initial values come from the SO, never from a temporary weapon instance.
        if (data is BarrierDataSO barrier)
        {
            AddInitialValue(preview, "upgrade.stat.charge_time",
                WeaponBase.CalculateLevelStat(data, SkillStatType.ChargeTime, barrier.chargeTime, firstLevel));
            AddInitialValue(preview, "upgrade.stat.knockback_force",
                WeaponBase.CalculateLevelStat(data, SkillStatType.KnockbackForce, barrier.knockbackForce, firstLevel));
            AddInitialValue(preview, "upgrade.stat.area", barrier.knockbackRadius);
            return;
        }

        long baseDamage = data is SpreadingFlameDataSO flame ? flame.damagePerTick : data.baseDamage;
        preview.Lines.Add(new UpgradePreviewLine("upgrade.stat.damage", "-",
            WeaponBase.CalculateLevelDamage(data, baseDamage, firstLevel).ToString()));

        switch (data)
        {
            case OrbWeaponDataSO orb:
                AddInitialCount(preview, data, "upgrade.stat.orb_count", SkillStatType.ProjectileCount, orb.orbCount);
                AddInitialValue(preview, "upgrade.stat.orbit_area", orb.radius);
                return;
            case AuraDataSO aura:
                AddInitialValue(preview, "upgrade.stat.area",
                    WeaponBase.CalculateLevelStat(data, SkillStatType.Radius, aura.baseRadius, firstLevel));
                return;
            case SpreadingFlameDataSO spreading:
                AddInitialCount(preview, data, "upgrade.stat.amount", SkillStatType.ProjectileCount, Mathf.Max(1, spreading.flameCount));
                AddInitialValue(preview, "upgrade.stat.area",
                    WeaponBase.CalculateLevelStat(data, SkillStatType.Radius, spreading.spreadRadius, firstLevel));
                AddInitialValue(preview, "upgrade.stat.cooldown", Mathf.Max(0.1f, spreading.baseCooldown));
                return;
            case TurretWeaponDataSO turret:
                AddInitialCount(preview, data, "upgrade.stat.amount", SkillStatType.ProjectileCount, turret.bulletNumber);
                AddInitialValue(preview, "upgrade.stat.area", data.hitRadius);
                preview.Lines.Add(new UpgradePreviewLine("upgrade.stat.max_hits", "-",
                    FormatMaxHits(data.maxHits), false, data.maxHits < 0));
                break;
            case MeteorWeaponSO meteor:
                AddInitialCount(preview, data, "upgrade.stat.amount", SkillStatType.ProjectileCount, meteor.MeteorNumber);
                AddInitialValue(preview, "upgrade.stat.explosion_area", data.hitRadius);
                AddInitialValue(preview, "upgrade.stat.cooldown", data.baseCooldown);
                return;
            case ChainLightningDataSO chain:
                AddInitialCount(preview, data, "upgrade.stat.chain_count", SkillStatType.ChainCount, chain.chainCount);
                break;
            case EvolvedFireballDataSO evolved:
                AddInitialValue(preview, "upgrade.stat.amount", evolved.bulletNumber);
                break;
            case ElementalFireballSO elemental:
                AddInitialValue(preview, "upgrade.stat.amount", elemental.bulletNumber);
                AddInitialValue(preview, "upgrade.stat.explosion_area", elemental.FireBoomSize);
                break;
            case CelestialFireballSO celestial:
                AddInitialValue(preview, "upgrade.stat.amount", celestial.bulletNumber);
                AddInitialValue(preview, "upgrade.stat.chain_count", celestial.chainCount);
                AddInitialValue(preview, "upgrade.stat.explosion_area", celestial.FireBoomSize);
                break;
        }

        AddInitialValue(preview, "upgrade.stat.cooldown", data.baseCooldown);
        AddInitialValue(preview, "upgrade.stat.projectile_speed", data.projectileSpeed);
    }

    private static void AddInitialValue(UpgradePreviewData preview, string statName, float value)
    {
        preview.Lines.Add(new UpgradePreviewLine(statName, "-", value.ToString("0.##")));
    }

    private static void AddInitialCount(UpgradePreviewData preview, WeaponDataSO data,
        string statName, SkillStatType stat, int baseCount)
    {
        preview.Lines.Add(new UpgradePreviewLine(statName, "-",
            WeaponBase.CalculateCount(data, stat, baseCount, 1).ToString()));
    }

    private static UpgradePreviewData CreateStatPreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = CreateBasePreview(upgrade);
        string statName = GetStatBonusName(upgrade.upgradeType);
        var calculator = PlayerStatsCalculate.Instance;

        if (calculator == null ||
            !calculator.TryGetRealTimeBonus(upgrade.upgradeType, upgrade.bonusMode, out float current))
        {
            // Missing state or an unsupported card is not a known zero bonus.
            preview.Lines.Add(new UpgradePreviewLine(statName, "-", "-"));
            return preview;
        }

        preview.Lines.Add(new UpgradePreviewLine(
            statName,
            FormatStatBonus(current, upgrade),
            FormatStatBonus(current + upgrade.statValue, upgrade)
        ));
        return preview;
    }

    private static UpgradePreviewData CreateBasePreview(UpgradeScriptableObject upgrade)
    {
        return new UpgradePreviewData
        {
            ShortDescription = upgrade.GetCurrentShortDescription(),
            Description = upgrade.GetCurrentDescription()
        };
    }

    private static string GetStatBonusName(UpgradeScriptableObject.UpgradeType type)
    {
        switch (type)
        {
            case UpgradeScriptableObject.UpgradeType.Stat_MoveSpeed: return "upgrade.stat.move_speed_bonus";
            case UpgradeScriptableObject.UpgradeType.Stat_Might: return "upgrade.stat.damage_bonus";
            case UpgradeScriptableObject.UpgradeType.Stat_MaxHealth: return "upgrade.stat.max_health_bonus";
            case UpgradeScriptableObject.UpgradeType.Stat_Heal: return "upgrade.stat.health_regen_bonus";
            case UpgradeScriptableObject.UpgradeType.Stat_Cooldown: return "upgrade.stat.cooldown_bonus";
            case UpgradeScriptableObject.UpgradeType.Stat_Growth: return "upgrade.stat.growth_bonus";
            default: return "upgrade.stat.effect";
        }
    }

    private static string FormatStatBonus(float value, UpgradeScriptableObject upgrade)
    {
        string suffix = upgrade.bonusMode == BonusMode.Percent
            ? "%"
            : upgrade.upgradeType == UpgradeScriptableObject.UpgradeType.Stat_Cooldown ? "%p" : "";
        string number = value > 0f ? $"+{value:0.##}" : value.ToString("0.##");
        return number + suffix;
    }

    private static string FormatMaxHits(int maxHits)
    {
        return maxHits < 0 ? "upgrade.value.unlimited" : maxHits.ToString();
    }
}

[System.Serializable]
public class UpgradePreviewLine
{
    public string StatNameKey;
    public string CurrentValue;
    public string NextValue;
    public bool LocalizeCurrentValue;
    public bool LocalizeNextValue;

    public UpgradePreviewLine(
        string statNameKey,
        string currentValue,
        string nextValue,
        bool localizeCurrentValue = false,
        bool localizeNextValue = false
    )
    {
        StatNameKey = statNameKey;
        CurrentValue = currentValue;
        NextValue = nextValue;
        LocalizeCurrentValue = localizeCurrentValue;
        LocalizeNextValue = localizeNextValue;
    }
}

public class UpgradePreviewData
{
    public string ShortDescription;
    public string Description;
    public List<UpgradePreviewLine> Lines = new List<UpgradePreviewLine>();
}
