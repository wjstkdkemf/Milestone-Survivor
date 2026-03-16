using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "PowerUp")]
public class PowerUpScriptableObject : ScriptableObject
{
    public PowerUpType powerUpType; // Enum type for power-ups
    public Sprite IconSprite;
    public string powerUpName; // Display name
    public string description; // Tooltip or UI description
    public float[] costPerLevel; // Base cost per level
    public int CurrentLevel;
    public bool isPercentage = true; // if true add "%" in the tool tip
    public float[] upgradeValues; // Upgrade values for each level
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