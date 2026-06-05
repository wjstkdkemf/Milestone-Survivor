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
        // 물리 엔진 대신 초고속 광역 레이더 가동
        EnemySwarmSystem.Instance.GetEnemiesInRadius(transform.position, searchRadius, searchResults);

        if (searchResults.Count == 0) return; // 주변에 적이 없음

        FindTargetPoints(); // 타겟팅 좌표 계산

        pendingMeteors = currentMeteorNumber;
        volleyTimer = 0f; // 첫 발은 즉시 발사
    }

    private void FindTargetPoints()
    {
        currentTargets.Clear();

        Vector3 densestPoint = GetDensestPoint();
        currentTargets.Add(densestPoint);

        for (int i = 1; i < currentMeteorNumber; i++)
        {
            int randomIdx = searchResults[Random.Range(0, searchResults.Count)];
            
            if (randomIdx < EnemySwarmSystem.Instance.activeEnemies.Count)
            {
                Enemy randomEnemy = EnemySwarmSystem.Instance.activeEnemies[randomIdx];
                if (randomEnemy != null) currentTargets.Add(randomEnemy.transform.position);
            }
            else
            {
                currentTargets.Add(densestPoint); // 에러 시 밀집 지점으로 쏨
            }
        }
    }

    private Vector3 GetDensestPoint()
    {
        int maxDensity = 0;
        Vector3 bestPoint = EnemySwarmSystem.Instance.activeEnemies[searchResults[0]].transform.position;

        int sampleCount = Mathf.Min(searchResults.Count, 5); 
        List<int> densityResults = new List<int>(50);

        for (int i = 0; i < sampleCount; i++)
        {
            int randomIdx = searchResults[Random.Range(0, searchResults.Count)];
            if (randomIdx >= EnemySwarmSystem.Instance.activeEnemies.Count) continue;

            Vector3 checkPos = EnemySwarmSystem.Instance.activeEnemies[randomIdx].transform.position;
            
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
        Vector3 target = currentTargets[currentTargets.Count - pendingMeteors];

        GameObject meteorObj = ObjectPoolingManager.Instance.spawnGameObject(MeteorPrefab, target, Quaternion.identity);
        if (meteorObj != null && meteorObj.TryGetComponent<Meteor>(out var meteorSkill))
        {
            float finalDamage = currentDamage + (PlayerStats.Instance != null ? PlayerStats.Instance.DamageBonus : 0);
            meteorSkill.Fire(target, finalDamage, currentHitRadius , WeaponID);
            meteorSkill.warningDuration = this.warningDuration;
        }

        pendingMeteors--;

        if (pendingMeteors > 0)
        {
            volleyTimer = volleyDelay;
        }
        else
        {
            cooldownTimer = currentCooldownTime; // 다 쐈으면 다시 무기 쿨타임 시작
        }
    }

    public override void LevelUp()
    {
        currentMeteorNumber++;
        currentDamage += 2;
    }

    public override UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = base.GetUpgradePreview(upgrade);

        preview.Lines.Add(new UpgradePreviewLine(
            "낙하 수",
            currentMeteorNumber.ToString(),
            (currentMeteorNumber + 1).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "피해량",
            currentDamage.ToString(),
            (currentDamage + 2).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "폭발 범위",
            currentHitRadius.ToString("0.##"),
            currentHitRadius.ToString("0.##")
        ));

        return preview;
    }
}
