using System.Collections.Generic;
using UnityEngine;

public class SpreadingFlameWeapon : WeaponBase
{
    private readonly List<int> searchResults = new List<int>(128);
    private readonly List<Enemy> targetCandidates = new List<Enemy>(128);

    private float searchRadius;
    private int flameCount;
    private float flameDuration;
    private float tickInterval;
    private float spreadRadius;
    private float spreadDelay;
    private int flameCountIncrease;
    private long damageIncrease;
    private float spreadRadiusIncrease;
    private GameObject flameEffectPrefab;
    private float cooldownTimer;
    private int pendingFlames;
    [SerializeField] private RangeCircleIndicator rangeIndicator;

    public override void Initialize(WeaponDataSO data)
    {
        if (!(data is SpreadingFlameDataSO flameData))
        {
            Debug.LogError("SpreadingFlameWeapon requires SpreadingFlameDataSO.");
            return;
        }

        base.Initialize(data);

        myData = data;
        WeaponID = flameData.WeaponId;
        searchRadius = flameData.searchRadius;
        flameCount = Mathf.Max(1, flameData.flameCount);
        flameDuration = Mathf.Max(0.1f, flameData.flameDuration);
        tickInterval = Mathf.Max(0.1f, flameData.tickInterval);
        spreadRadius = flameData.spreadRadius;
        spreadDelay = Mathf.Max(0f, flameData.spreadDelay);
        currentCooldown = Mathf.Max(0.1f, flameData.baseCooldown);
        currentDamage = flameData.damagePerTick;
        flameCountIncrease = Mathf.Max(1, flameData.flameCountIncrease);
        damageIncrease = flameData.damageIncrease;
        spreadRadiusIncrease = flameData.spreadRadiusIncrease;
        flameEffectPrefab = flameData.flameEffectPrefab;
        cooldownTimer = 0f;
        pendingFlames = 0;

        attackMotion = flameData.attackMotion;

        SetupRangeIndicator();
    }

    public override void OnUpdate()
    {
        if (EnemySwarmSystem.Instance == null || flameEffectPrefab == null)
            return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        if (AttachFlames())
            cooldownTimer = GetCooldown(currentCooldown);
    }

    private bool AttachFlames()
    {
        EnemySwarmSystem.Instance.GetEnemiesInRadius(transform.position, searchRadius, searchResults);
        BuildTargetCandidates();

        if (pendingFlames == 0)
            pendingFlames = Mathf.Min(flameCount, targetCandidates.Count);

        int attachCount = Mathf.Min(pendingFlames, targetCandidates.Count);
        for (int i = 0; i < attachCount; i++)
        {
            int randomIndex = Random.Range(0, targetCandidates.Count);
            Enemy target = targetCandidates[randomIndex];
            targetCandidates.RemoveAt(randomIndex);

            if (!SpawnFlame(target)) return false;
            pendingFlames--;
        }

        // No eligible targets ends the volley; a failed spawn above keeps the remainder.
        pendingFlames = 0;
        return true;
    }

    private void BuildTargetCandidates()
    {
        targetCandidates.Clear();

        for (int i = 0; i < searchResults.Count; i++)
        {
            Enemy enemy = EnemySwarmSystem.Instance.GetEnemyByIndex(searchResults[i]);
            if (enemy == null || enemy.currentNormalState == Enemy.EnemyState.Dead)
                continue;

            if (SpreadingFlameEffect.IsBurning(enemy))
                continue;

            targetCandidates.Add(enemy);
        }
    }

    private bool SpawnFlame(Enemy target)
    {
        if (ObjectPoolingManager.Instance == null)
            return false;

        GameObject flameObj = ObjectPoolingManager.Instance.spawnGameObject(
            flameEffectPrefab,
            target.transform.position,
            Quaternion.identity
        );

        if (flameObj == null)
            return false;

        if (flameObj.TryGetComponent<SpreadingFlameEffect>(out var flame))
        {
            long finalDamage = GetDamage();
            flame.Attach(target, finalDamage, flameDuration, tickInterval, spreadRadius, spreadDelay, WeaponID);
            return true;
        }
        else
        {
            ObjectPoolingManager.Instance.ReturnObjectToPool(flameObj);
            return false;
        }
    }

    public long GetDamage()
    {
        return CalculateDamage(currentDamage);
    }

    protected override void RecalculateLevelStats()
    {
        if (!(myData is SpreadingFlameDataSO data)) return;
        int upgrades = CurrentLevel - 1;
        flameCount = Mathf.Max(1, data.flameCount) + flameCountIncrease * upgrades;
        currentDamage = data.damagePerTick + damageIncrease * upgrades;
        spreadRadius = data.spreadRadius + spreadRadiusIncrease * upgrades;

        SetupRangeIndicator();
    }

    public override UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = base.GetUpgradePreview(upgrade);

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.amount",
            flameCount.ToString(),
            (flameCount + flameCountIncrease).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.damage",
            currentDamage.ToString(),
            (currentDamage + damageIncrease).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.area",
            spreadRadius.ToString("0.##"),
            (spreadRadius + spreadRadiusIncrease).ToString("0.##")
        ));

        return preview;
    }
    private void SetupRangeIndicator()
    {
        if (rangeIndicator == null)
            rangeIndicator = GetComponentInChildren<RangeCircleIndicator>();

        if (rangeIndicator != null)
            rangeIndicator.SetRadius(searchRadius);
    }
}
