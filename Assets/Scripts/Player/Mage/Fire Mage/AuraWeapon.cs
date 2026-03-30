using UnityEngine;

public class AuraWeapon : WeaponBase
{
    // [런타임 상태 변수]
    private float currentRadius;
    private float currentBaseDamage;
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
            currentBaseDamage = auraData.baseDamage;
            applySlow = auraData.applySlow;
            slowPercentage = auraData.slowPercentage;

            if (auraData.auraPrefab != null)
            {
                auraInstance = Instantiate(auraData.auraPrefab, transform.position, Quaternion.identity, transform);
                
                auraScript = auraInstance.GetComponent<AuraZone>();
                if (auraScript != null)
                {
                    auraScript.SetAuraInfo(currentTickRate, GetDamage(), applySlow, slowPercentage);
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
    public float GetDamage()
    {
        float bonus = (playerStats != null) ? playerStats.DamageBonus : 0;
        return currentBaseDamage + bonus; // 필요시 계수(Scaling) 추가 가능
    }

    // [핵심] 오라 크기 변경 로직
    private void UpdateAuraSize()
    {
        if (auraInstance != null)
        {
            // localScale을 조절하면, 오라 프리팹에 있는 Collider2D의 반경과
            // 시각적 이펙트(Particle, Sprite 등)의 크기가 동시에 커집니다.
            auraInstance.transform.localScale = new Vector3(currentRadius, currentRadius, 1f);
        }
    }

    // 레벨업 시 범위와 데미지 증가
    public override void LevelUp()
    {
        currentBaseDamage += 2f;    // 데미지 증가
        currentRadius += 1.5f;      // 반경(크기) 증가!

        // 변경된 데미지 오라에 실시간 갱신
        if (auraScript != null)
        {
            auraScript.SetAuraInfo(currentTickRate, GetDamage(), applySlow, slowPercentage);
        }
        
        // 커진 반경 적용
        UpdateAuraSize();

        Debug.Log($"[Aura Level Up] 데미지: {currentBaseDamage}, 반경: {currentRadius}");
    }
}