using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class MeteorWeapon : WeaponBase
{
    private int currentMeteorNumber;
    private float currentCooldownTime;
    private float warningDuration;
    private float volleyDelay; 
    private float searchRadius;
    private float densityCheckRadius;

    private GameObject MeteorPrefab;
    
    // 타이머 및 상태 변수
    private float cooldownTimer; 
    private float volleyTimer;
    private int pendingMeteors = 0; // 아직 덜 쏜 메테오 개수
    private List<Vector3> currentTargets = new List<Vector3>();

    // 가비지 방지용 바구니
    private List<int> searchResults = new List<int>(200);
    private readonly List<Vector3> candidatePositions = new List<Vector3>(200);
    private readonly List<int> densityResults = new List<int>(50);

    public override void Initialize(WeaponDataSO data)
    {
        if (data is MeteorWeaponSO MeteorData)
        {
            base.Initialize(data);
            
            currentMeteorNumber = MeteorData.MeteorNumber;
            currentCooldownTime = MeteorData.baseCooldown;
            warningDuration = MeteorData.warningDuration;
            volleyDelay = MeteorData.volleyDelay;
            searchRadius = MeteorData.range;
            densityCheckRadius = MeteorData.densityCheckRadius;
            currentDamage = MeteorData.baseDamage;
            MeteorPrefab = MeteorData.MeteorPrefab;

            attackMotion = MeteorData.attackMotion;

            this.WeaponID = data.WeaponId;
            
            cooldownTimer = 0f; 
        }
    }

    public override void OnUpdate()
    {
        if (pendingMeteors <= 0)
        {
            if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                ActivateMeteorStrike();
            }
        }
        else
        {
            volleyTimer -= Time.deltaTime;
            if (volleyTimer <= 0f)
            {
                FireSingleMeteor();
            }
        }
    }

    void ActivateMeteorStrike()
    {
        if (EnemySwarmSystem.Instance == null) return;
        // 물리 엔진 대신 초고속 광역 레이더 가동
        EnemySwarmSystem.Instance.GetEnemiesInRadius(transform.position, searchRadius, searchResults);

        if (searchResults.Count == 0) return; // 주변에 적이 없음

        FindTargetPoints(); // 타겟팅 좌표 계산

        pendingMeteors = currentTargets.Count;
        volleyTimer = 0f; // 첫 발은 즉시 발사
    }

    private void FindTargetPoints()
    {
        currentTargets.Clear();
        candidatePositions.Clear();
        foreach (int index in searchResults)
        {
            Enemy enemy = EnemySwarmSystem.Instance.TryGetTarget(index);
            if (enemy != null) candidatePositions.Add(enemy.transform.position);
        }
        if (candidatePositions.Count == 0 || currentMeteorNumber <= 0) return;

        Vector3 densestPoint = GetDensestPoint();
        currentTargets.Add(densestPoint);

        for (int i = 1; i < currentMeteorNumber; i++)
        {
            currentTargets.Add(candidatePositions[Random.Range(0, candidatePositions.Count)]);
        }
    }

    private Vector3 GetDensestPoint()
    {
        int maxDensity = 0;
        Vector3 bestPoint = candidatePositions[0];

        int sampleCount = Mathf.Min(candidatePositions.Count, 5);

        for (int i = 0; i < sampleCount; i++)
        {
            Vector3 checkPos = candidatePositions[Random.Range(0, candidatePositions.Count)];
            
            EnemySwarmSystem.Instance.GetEnemiesInRadius(checkPos, densityCheckRadius, densityResults);
            
            if (densityResults.Count > maxDensity)
            {
                maxDensity = densityResults.Count;
                bestPoint = checkPos;
            }
        }

        return bestPoint;
    }

    private void FireSingleMeteor()
    {
        if (pendingMeteors <= 0 || pendingMeteors > currentTargets.Count)
        {
            pendingMeteors = 0;
            return;
        }
        Vector3 target = currentTargets[currentTargets.Count - pendingMeteors];

        GameObject meteorObj = ObjectPoolingManager.Instance.spawnGameObject(MeteorPrefab, target, Quaternion.identity);
        if (meteorObj != null && meteorObj.TryGetComponent<Meteor>(out var meteorSkill))
        {
            long finalDamage = GetDamage();
            meteorSkill.warningDuration = this.warningDuration;
            meteorSkill.Fire(target, finalDamage, currentHitRadius , WeaponID);
        }

        pendingMeteors--;

        if (pendingMeteors > 0)
        {
            volleyTimer = volleyDelay;
        }
        else
        {
            cooldownTimer = GetCooldown(currentCooldownTime); // 다 쐈으면 다시 무기 쿨타임 시작
        }
    }

    public long GetDamage()
    {
        return CalculateDamage(currentDamage);
    }

    protected override void RecalculateLevelStats()
    {
        if (!(myData is MeteorWeaponSO data)) return;
        int upgrades = CurrentLevel - 1;
        currentMeteorNumber = data.MeteorNumber + upgrades;
        currentDamage = data.baseDamage + 2L * upgrades;
    }

    public override UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = base.GetUpgradePreview(upgrade);

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.amount",
            currentMeteorNumber.ToString(),
            (currentMeteorNumber + 1).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.damage",
            currentDamage.ToString(),
            (currentDamage + 2).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.explosion_area",
            currentHitRadius.ToString("0.##"),
            currentHitRadius.ToString("0.##")
        ));

        return preview;
    }
}
