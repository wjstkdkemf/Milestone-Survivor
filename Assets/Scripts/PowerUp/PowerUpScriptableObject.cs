using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "PowerUp")]
public class PowerUpScriptableObject : ScriptableObject
{
    public PowerUpType powerUpType; // Enum type for power-ups
    public Sprite IconSprite;
    public string powerUpName; // Display name
    public string description; // Tooltip or UI description
    [Header("Localization")]
    [SerializeField] private LocalizedString localizedPowerUpName;
    [SerializeField] private LocalizedString localizedDescription;
    public long[] costPerLevel; // Base cost per level
    public int CurrentLevel;
    public bool isPercentage = true; // if true add "%" in the tool tip
    public float[] upgradeValues; // Upgrade values for each level

    public string GetLocalizedName()
    {
        return GetLocalizedString(localizedPowerUpName, powerUpName);
    }

    public string GetLocalizedDescription()
    {
        return GetLocalizedString(localizedDescription, description);
    }

    private static string GetLocalizedString(LocalizedString localizedString, string fallback)
    {
        if (localizedString != null && !localizedString.IsEmpty)
        {
            string localized = localizedString.GetLocalizedString();

            if (!string.IsNullOrEmpty(localized))
                return localized;
        }

        return fallback ?? string.Empty;
    }
}
[System.Serializable]
public enum PowerUpType
{
    MaxHealth,
    Damage,
    DamageRation,
    Armor,
    KnockBack,
    HealthRegeneration,
    DobleDamageChance,
    CooldownReduction,
    projectilespeed,
    XPBoost,
    luckBoost,
    MovementSpeed,
    Encounter,
    UpgradeCount
}
