using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbWeapon : WeaponBase // 부모 클래스 변경
{
    private int currentOrbCount;
    private float currentRadius;
    private float currentRotationSpeed;
    private float currentScaling = 1.0f;

    private GameObject orbPrefab; // 작은 구체 프리팹
    protected List<GameObject> spawnedOrbs = new List<GameObject>();
    private float angleStep;

    void Start()
    {
        PlayerStatsCalculate.Instance.AttackChanged += RefreshDamage;
        RefreshDamage();
    }
    void OnDestroy()
    {
        PlayerStatsCalculate.Instance.AttackChanged -= RefreshDamage;
    }
    // 초기화: 플레이어가 무기를 획득했을 때 딱 1번 실행됨

    public override void Initialize(WeaponDataSO data)
    {
        if (data is OrbWeaponDataSO orbData)
        {
            currentOrbCount = orbData.orbCount;
            currentRadius = orbData.radius;
            currentRotationSpeed = orbData.rotationSpeed;
            currentDamage = orbData.baseDamage; // 부모 SO에 있는 데미지

            currentHitRadius = orbData.hitRadius;

            attackMotion = orbData.attackMotion;

            this.WeaponID = orbData.WeaponId;

            orbPrefab = orbData.orbProjectilePrefab;
        }
        else
        {
            Debug.LogError("잘못된 데이터가 들어왔습니다! OrbWeaponDataSO가 필요합니다.");
            return;
        }
        SpawnOrbs();
    }

    // 플레이어 컨트롤러가 매 프레임 호출해줌
    public override void OnUpdate()
    {
        OrbitOrbs();
    }

    void SpawnOrbs()
    {
        ClearOrbs();

        if (currentOrbCount == 0) return;

        angleStep = 360f / currentOrbCount;

        for (int i = 0; i < currentOrbCount; i++)
        {
            GameObject orb = Instantiate(orbPrefab, transform.position, Quaternion.identity, transform);
            
            float angle = i * angleStep;
            orb.transform.position = GetOrbPosition(angle);
            
            SetupSpawnedOrb(orb);
            spawnedOrbs.Add(orb);
        }
    }
    Vector3 GetOrbPosition(float angle)
    {
        float angleInRadians = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(angleInRadians) * currentRadius;
        float y = Mathf.Sin(angleInRadians) * currentRadius;

        return new Vector3(x, y, 0) + transform.position;
    }
    protected virtual void SetupSpawnedOrb(GameObject orb)
    {
        if (orb.TryGetComponent<Orb>(out var orbScript))
        {
            orbScript.SetInfo(GetDamage(), currentHitRadius, 0 , WeaponID);
        }
    }
    void OrbitOrbs()
    {
       for (int i = 0; i < spawnedOrbs.Count; i++)

        {
            if (spawnedOrbs[i] == null) continue;

            float angle = (i * angleStep) + Time.time * currentRotationSpeed;

            spawnedOrbs[i].transform.position = GetOrbPosition(angle);

        }
    }

    void ClearOrbs()
    {
        foreach (GameObject orb in spawnedOrbs)
        {
            if (orb != null) Destroy(orb);
        }
        spawnedOrbs.Clear();
    }
    public void UpgradeRadius(float amount)
    {
        currentRadius += amount;
    }

    public void UpgradeSpeed(float amount)
    {
        currentRotationSpeed += amount;
    }
    protected override void RecalculateLevelStats()
    {
        if (!(myData is OrbWeaponDataSO data)) return;
        currentOrbCount = CalculateCount(SkillStatType.ProjectileCount, data.orbCount, CurrentLevel);
        SpawnOrbs();
    }

    public override UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = base.GetUpgradePreview(upgrade);
        if (!(myData is OrbWeaponDataSO data)) return preview;

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.orb_count",
            currentOrbCount.ToString(),
            CalculateCount(SkillStatType.ProjectileCount, data.orbCount, CurrentLevel + 1).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.orbit_area",
            currentRadius.ToString("0.##"),
            currentRadius.ToString("0.##")
        ));

        return preview;
    }
    public long GetDamage()
    {
        return CalculateDamage(currentDamage, currentScaling);
    }
    protected override void RefreshDamage()
    {
        long damage = GetDamage();

        foreach (GameObject orb in spawnedOrbs)
        {
            if (orb != null && orb.TryGetComponent<Orb>(out var orbScript))
                orbScript.damage = damage;
        }
    }
}
