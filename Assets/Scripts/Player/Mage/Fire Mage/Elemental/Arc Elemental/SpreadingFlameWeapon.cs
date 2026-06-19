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

        AttachFlames();
        cooldownTimer = currentCooldown;
    }

    private void AttachFlames()
    {
        EnemySwarmSystem.Instance.GetEnemiesInRadius(transform.position, searchRadius, searchResults);
        BuildTargetCandidates();

        int attachCount = Mathf.Min(flameCount, targetCandidates.Count);
        for (int i = 0; i < attachCount; i++)
        {
            int randomIndex = Random.Range(0, targetCandidates.Count);
            Enemy target = targetCandidates[randomIndex];
            targetCandidates.RemoveAt(randomIndex);

            SpawnFlame(target);
        }
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

    private void SpawnFlame(Enemy target)
    {
        if (ObjectPoolingManager.Instance == null)
            return;

        GameObject flameObj = ObjectPoolingManager.Instance.spawnGameObject(
            flameEffectPrefab,
            target.transform.position,
            Quaternion.identity
        );

        if (flameObj == null)
            return;

        if (flameObj.TryGetComponent<SpreadingFlameEffect>(out var flame))
        {
            float finalDamage = currentDamage + (PlayerStats.Instance != null ? PlayerStats.Instance.DamageBonus : 0);
            flame.Attach(target, finalDamage, flameDuration, tickInterval, spreadRadius, spreadDelay, WeaponID);
        }
        else
        {
            ObjectPoolingManager.Instance.ReturnObjectToPool(flameObj);
        }
    }

    public override void LevelUp()
    {
        flameCount += flameCountIncrease;
        currentDamage += damageIncrease;
        spreadRadius += spreadRadiusIncrease;

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
