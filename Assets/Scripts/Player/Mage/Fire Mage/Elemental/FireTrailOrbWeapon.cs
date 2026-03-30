using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrailOrbWeapon : OrbWeapon
{
// 장판 관련 런타임 데이터
    private GameObject trailPrefab;
    private float spawnInterval;
    private float trailDuration;
    private float trailDamageScaling;

    public override void Initialize(WeaponDataSO data)
    {
        // 1. 부모(OrbWeapon)의 초기화 먼저 실행 (기본 데이터 로드)
        base.Initialize(data);

        // 2. 장판 전용 데이터 추가 로드
        if (data is FireTrailOrbDataSO fireData)
        {
            this.trailPrefab = fireData.trailPrefab;
            this.spawnInterval = fireData.spawnInterval;
            this.trailDuration = fireData.trailDuration;
            this.trailDamageScaling = fireData.trailDamageScaling;
        }
        // SpawnOrbs는 base.Initialize 안에서 이미 호출되었으므로,
        // 데이터 로드 후 첫 생성된 오즈들에 정보를 업데이트해줘야 합니다.
        UpdateTrailInfoForExistingOrbs();
    }

    // 부모 클래스에서 수정한 가상 함수 오버라이드
    protected override void SetupSpawnedOrb(GameObject orb)
    {
        // 1. 부모의 기본 셋업(데미지 주입 등) 실행
        base.SetupSpawnedOrb(orb);

        // 2. FireTrailOrb 컴포넌트를 찾아 장판 정보 추가 주입
        if (orb.TryGetComponent<FireTrailOrb>(out var fireOrbScript))
        {
            float finalTrailDamage = GetDamage() * trailDamageScaling;
            fireOrbScript.SetTrailInfo(trailPrefab, spawnInterval, trailDuration, finalTrailDamage);
        }
    }

    // Initialize 시점이나 데미지 업그레이드 시 호출
    private void UpdateTrailInfoForExistingOrbs()
    {
        float finalTrailDamage = GetDamage() * trailDamageScaling;
        foreach (var orbObj in spawnedOrbs)
        {
            if (orbObj != null && orbObj.TryGetComponent<FireTrailOrb>(out var fireOrbScript))
            {
                fireOrbScript.SetTrailInfo(trailPrefab, spawnInterval, trailDuration, finalTrailDamage);
            }
        }
    }

    public override void UpgradeDamage(float amount)
    {
        base.UpgradeDamage(amount);
        // 데미지가 바뀌었으니 장판 데미지도 갱신
        UpdateTrailInfoForExistingOrbs();
    }
}
