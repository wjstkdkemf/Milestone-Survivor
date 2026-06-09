using UnityEngine;

public class AuraWeapon : WeaponBase
{
    // [런타임 상태 변수]
    private float currentRadius;
    private float currentTickRate;
    private bool applySlow;
    private float slowPercentage;

    // [내부 변수]
    private GameObject auraInstance;
    private AuraZone auraScript;
    private PlayerStats playerStats;

    public override void Initialize(WeaponDataSO data)
    {
        if (data is AuraDataSO auraData)
        {
            currentRadius = auraData.baseRadius;
            currentTickRate = auraData.tickRate;
            currentDamage = auraData.baseDamage;
            applySlow = auraData.applySlow;
            slowPercentage = auraData.slowPercentage;

            this.WeaponID = auraData.WeaponId;

            if (auraData.auraPrefab != null)
            {
                auraInstance = Instantiate(auraData.auraPrefab, transform.position, Quaternion.identity, transform);
                
                auraScript = auraInstance.GetComponent<AuraZone>();
                if (auraScript != null)
                {
                    auraScript.SetupAura(currentTickRate, GetDamage(), currentRadius, applySlow, slowPercentage , this.WeaponID);
                }

                UpdateAuraSize();
            }
        }
        else
        {
            Debug.LogError("잘못된 데이터! AuraDataSO가 필요합니다.");
        }

        if (PlayerStats.Instance != null) playerStats = PlayerStats.Instance;
        else playerStats = GetComponentInParent<PlayerStats>();
    }

    public override void OnUpdate()
    {
        // 오라는 상시 켜져 있으므로, Update에서 쿨타임을 재거나 발사할 필요가 없습니다.
        // 타격 연산은 오라 자체(AuraZone)가 알아서 처리합니다.
    }

    // 데미지 계산식
    public long GetDamage()
    {
        long bonus = (PlayerStats.Instance != null) ? PlayerStats.Instance.DamageBonus : 0;
        return currentDamage + bonus;
    }

    private void UpdateAuraSize()
    {
        if (auraInstance != null)
        {
            auraInstance.transform.localScale = new Vector3(currentRadius, currentRadius, 1f);
        }
    }

    // 레벨업 시 범위와 데미지 증가
    public override void LevelUp()
    {
        currentDamage += 2;
        currentRadius += 1.5f;

        if (auraScript != null)
        {
            auraScript.SetupAura(currentTickRate, GetDamage(), currentRadius, applySlow, slowPercentage , WeaponID);
        }
        UpdateAuraSize();
    }

    public override UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = base.GetUpgradePreview(upgrade);

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.damage",
            currentDamage.ToString(),
            (currentDamage + 2).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.area",
            currentRadius.ToString("0.##"),
            (currentRadius + 1.5f).ToString("0.##")
        ));

        return preview;
    }
}
