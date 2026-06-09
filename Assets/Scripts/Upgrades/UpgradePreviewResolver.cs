using System.Collections.Generic;
using UnityEngine;

public class UpgradePreviewResolver : MonoBehaviour
{
    [SerializeField] private PlayerWeaponController weaponManager;
    [SerializeField] private PlayerStats playerStats;

    private void Start()
    {
        playerStats = PlayerStats.Instance;
    }

    public UpgradePreviewData Resolve(UpgradeScriptableObject upgrade)
    {
        if (upgrade == null)
            return null;

        if (upgrade.upgradeType == UpgradeScriptableObject.UpgradeType.Weapon &&
            upgrade.linkedWeaponData != null &&
            weaponManager != null &&
            weaponManager.TryGetWeapon(upgrade.linkedWeaponData, out WeaponBase weapon))
        {
            return weapon.GetUpgradePreview(upgrade);
        }

        return UpgradePreviewFactory.CreateFallbackPreview(upgrade, playerStats);
    }
}

public static class UpgradePreviewFactory
{
    public static UpgradePreviewData CreateFallbackPreview(UpgradeScriptableObject upgrade, PlayerStats playerStats)
    {
        if (upgrade == null)
            return null;

        if (upgrade.upgradeType == UpgradeScriptableObject.UpgradeType.Weapon &&
            upgrade.linkedWeaponData != null)
        {
            return CreateNewWeaponPreview(upgrade);
        }

        return CreateStatPreview(upgrade, playerStats);
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
        preview.Lines.Add(new UpgradePreviewLine("upgrade.stat.damage", "-", weaponData.baseDamage.ToString()));
        preview.Lines.Add(new UpgradePreviewLine("upgrade.stat.cooldown", "-", weaponData.baseCooldown.ToString("0.##")));
        preview.Lines.Add(new UpgradePreviewLine("upgrade.stat.area", "-", weaponData.hitRadius.ToString("0.##")));
        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.max_hits",
            "-",
            FormatMaxHits(weaponData.maxHits),
            false,
            weaponData.maxHits < 0
        ));
        preview.Lines.Add(new UpgradePreviewLine("upgrade.stat.projectile_speed", "-", weaponData.projectileSpeed.ToString("0.##")));

        return preview;
    }

    private static UpgradePreviewData CreateStatPreview(UpgradeScriptableObject upgrade, PlayerStats playerStats)
    {
        UpgradePreviewData preview = CreateBasePreview(upgrade);

        switch (upgrade.upgradeType)
        {
            case UpgradeScriptableObject.UpgradeType.Stat_MoveSpeed:
                AddStatLine(preview, "upgrade.stat.move_speed", playerStats != null ? playerStats.SpeedBonus : 0f, upgrade.statValue);
                break;
            case UpgradeScriptableObject.UpgradeType.Stat_Might:
                AddStatLine(preview, "upgrade.stat.damage", playerStats != null ? playerStats.DamageBonus : 0f, upgrade.statValue);
                break;
            case UpgradeScriptableObject.UpgradeType.Stat_Cooldown:
                AddStatLine(preview, "upgrade.stat.cooldown_reduction", playerStats != null ? playerStats.cooldownReduction : 0f, upgrade.statValue);
                break;
            case UpgradeScriptableObject.UpgradeType.Stat_Growth:
                AddStatLine(preview, "upgrade.stat.growth", playerStats != null ? playerStats.experienceBonus : 0f, upgrade.statValue);
                break;
            default:
                preview.Lines.Add(new UpgradePreviewLine("upgrade.stat.effect", "-", FormatSigned(upgrade.statValue)));
                break;
        }

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

    private static void AddStatLine(UpgradePreviewData preview, string statName, float current, float addValue)
    {
        preview.Lines.Add(new UpgradePreviewLine(
            statName,
            current.ToString("0.##"),
            (current + addValue).ToString("0.##")
        ));
    }

    private static string FormatSigned(float value)
    {
        return value >= 0f ? $"+{value:0.##}" : value.ToString("0.##");
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
