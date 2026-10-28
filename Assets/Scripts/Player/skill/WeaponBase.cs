using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponDataSO myData;
    public string WeaponID;
    public int CurrentLevel;
    protected long currentDamage;
    protected float currentCooldown;
    protected float currentHitRadius;
    protected int currentMaxHits;
    protected float currentProjectileSpeed;
    [SerializeField] protected WeaponMotionData attackMotion;
    protected WeaponAnimationController weaponAnimator;
    protected WeaponVisualController weaponVisual;

    public virtual void Initialize(WeaponDataSO data)
    {   
        currentHitRadius = data.hitRadius;
        currentMaxHits = data.maxHits;
        currentProjectileSpeed = data.projectileSpeed;

        weaponAnimator = GetComponentInParent<WeaponAnimationController>();
        weaponVisual = GetComponentInParent<WeaponVisualController>();
    }

    // 플레이어의 Update에서 매 프레임 호출해줄 함수
    public abstract void OnUpdate();
    public virtual void LevelUp()
    {
        RecalculateLevelStats();
    }

    public void SetLevel(int level)
    {
        level = Mathf.Max(1, level);
        if (CurrentLevel == level) return;

        CurrentLevel = level;
        RecalculateLevelStats();
    }

    // Recompute stats only; do not restart cooldowns, volleys, or Initialize().
    protected abstract void RecalculateLevelStats();

    protected float GetCooldown(float baseCooldown)
    {
        // Stats and UI use percentage points: 10 means a ten percent reduction.
        float reduction = PlayerStats.Instance != null ? PlayerStats.Instance.cooldownReduction : 0f;
        if (float.IsNaN(reduction) || float.IsInfinity(reduction)) reduction = 0f;
        return Mathf.Max(0.01f, baseCooldown * (1f - Mathf.Clamp(reduction, 0f, 100f) / 100f));
    }
    public virtual UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = new UpgradePreviewData();

        preview.ShortDescription = upgrade != null ? upgrade.GetCurrentShortDescription() : "";
        preview.Description = upgrade != null ? upgrade.GetCurrentDescription() : string.Empty;

        /*preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.status",
            "upgrade.value.equipped",
            "upgrade.value.upgrade",
            true,
            true
        ));*/

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.level",
            $"Lv.{CurrentLevel}",
            $"Lv.{CurrentLevel + 1}"
        ));

        return preview;
    }
    protected void PlayAttackMotion()
    {
        if (weaponAnimator != null && attackMotion != null)
            weaponAnimator.PlayMotion(attackMotion);
    }
    protected long CalculateDamage(long baseDamage, double attackScaling = 1d)
    {
        long attack = PlayerStats.Instance != null
            ? PlayerStats.Instance.DamageBonus
            : 0L;

        return baseDamage + (long)(attack * attackScaling);
    }
    protected float CalculateLevelStat(SkillStatType stat,
                                        float baseValue,
                                        int level)
    {
        return CalculateLevelStat(myData, stat, baseValue, level);
    }

    public static float CalculateLevelStat(WeaponDataSO data, SkillStatType stat, float baseValue, int level)
    {
        GetLevelGrowth(data, stat, level, out float flat, out float percent);
        return (baseValue + flat) * (1f + percent * 0.01f);
    }

    protected long CalculateLevelDamage(long baseDamage, int level)
    {
        return CalculateLevelDamage(myData, baseDamage, level);
    }

    public static long CalculateLevelDamage(WeaponDataSO data, long baseDamage, int level)
    {
        GetLevelGrowth(data, SkillStatType.Damage, level, out float flat, out float percent);

        // Keep the long base damage exact instead of narrowing it to float.
        decimal result = (baseDamage + (decimal)flat) * (1m + (decimal)percent * 0.01m);
        if (result >= long.MaxValue) return long.MaxValue;
        return result <= 0m ? 0L : (long)result;
    }

    private static void GetLevelGrowth(WeaponDataSO data, SkillStatType stat, int level, out float flat, out float percent)
    {
        int growthCount = Mathf.Max(0, level - 1);
        flat = 0f;
        percent = 0f;

        var rules = data.GrowthRules;

        if (rules != null)
        {
            foreach (var rule in rules)
            {
                if (rule.stat != stat)
                    continue;

                float growth = rule.valuePerLevel * growthCount;

                switch (rule.mode)
                {
                    case BonusMode.Flat:
                        flat += growth;
                        break;

                    case BonusMode.Percent:
                        percent += growth;
                        break;
                }
            }
        }
    }

    protected int CalculateCount(SkillStatType stat,
                                int baseCount,
                                int level)
    {
        return CalculateCount(myData, stat, baseCount, level);
    }

    public static int CalculateCount(WeaponDataSO data, SkillStatType stat, int baseCount, int level)
    {
        float result = CalculateLevelStat(data, stat, baseCount, level);

        if (result >= int.MaxValue) return int.MaxValue;
        return Mathf.FloorToInt(Mathf.Max(0f, result));
    }
    protected virtual void RefreshDamage()
    {
    }
}
