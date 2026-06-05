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

        preview.Lines.Add(new UpgradePreviewLine("상태", "-", "신규 장착"));
        preview.Lines.Add(new UpgradePreviewLine("피해량", "-", weaponData.baseDamage.ToString()));
        preview.Lines.Add(new UpgradePreviewLine("쿨타임", "-", weaponData.baseCooldown.ToString("0.##")));
        preview.Lines.Add(new UpgradePreviewLine("범위", "-", weaponData.hitRadius.ToString("0.##")));
        preview.Lines.Add(new UpgradePreviewLine("최대 타격", "-", FormatMaxHits(weaponData.maxHits)));
        preview.Lines.Add(new UpgradePreviewLine("투사체 속도", "-", weaponData.projectileSpeed.ToString("0.##")));

        return preview;
    }

    private static UpgradePreviewData CreateStatPreview(UpgradeScriptableObject upgrade, PlayerStats playerStats)
    {
        UpgradePreviewData preview = CreateBasePreview(upgrade);

        switch (upgrade.upgradeType)
        {
            case UpgradeScriptableObject.UpgradeType.Stat_MoveSpeed:
                AddStatLine(preview, "이동 속도", playerStats != null ? playerStats.SpeedBonus : 0f, upgrade.statValue);
                break;
            case UpgradeScriptableObject.UpgradeType.Stat_Might:
                AddStatLine(preview, "공격력", playerStats != null ? playerStats.DamageBonus : 0f, upgrade.statValue);
                break;
            case UpgradeScriptableObject.UpgradeType.Stat_Cooldown:
                AddStatLine(preview, "쿨타임 감소", playerStats != null ? playerStats.cooldownReduction : 0f, upgrade.statValue);
                break;
            case UpgradeScriptableObject.UpgradeType.Stat_Growth:
                AddStatLine(preview, "경험치 보너스", playerStats != null ? playerStats.experienceBonus : 0f, upgrade.statValue);
                break;
            default:
                preview.Lines.Add(new UpgradePreviewLine("효과", "-", FormatSigned(upgrade.statValue)));
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
        return maxHits < 0 ? "무제한" : maxHits.ToString();
    }
}

[System.Serializable]
public class UpgradePreviewLine
{
    public string StatName;
    public string CurrentValue;
    public string NextValue;

    public UpgradePreviewLine(string statName, string currentValue, string nextValue)
    {
        StatName = statName;
        CurrentValue = currentValue;
        NextValue = nextValue;
    }
}

public class UpgradePreviewData
{
    public string ShortDescription;
    public string Description;
    public List<UpgradePreviewLine> Lines = new List<UpgradePreviewLine>();
}
