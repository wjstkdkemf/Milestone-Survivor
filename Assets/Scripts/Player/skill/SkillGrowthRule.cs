public enum SkillStatType
{
    Damage = 0,
    ProjectileCount = 1,
    Radius = 2,
    ChainCount = 3,
    TrailDamageScaling = 4,
    ChargeTime = 5,
    KnockbackForce = 6
}

[System.Serializable]
public struct SkillGrowthRule
{
    public SkillStatType stat;
    public BonusMode mode;

    [UnityEngine.Tooltip(
        "렙 1당 올라가는 수치. Percent: 10 means 10%.")]
    public float valuePerLevel;
}
