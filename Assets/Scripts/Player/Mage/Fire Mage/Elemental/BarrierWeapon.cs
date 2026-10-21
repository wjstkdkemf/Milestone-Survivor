using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierWeapon : WeaponBase
{
    // [런타임 상태 변수]
    private float currentChargeTimeLimit;
    private float chargeTimer;
    private float currentKnockbackForce;
    private float currentKnockbackRadius;
    public bool isBarrierActive = false;

    // [내부 변수]
    private GameObject barrierInstance; // 생성된 배리어 객체
    private BarrierShield barrierScript; // 배리어 로직 스크립트
    private GameObject explosionEffect;

    private PlayerHealth playerHealthScript;

    private readonly List<int> knockbackTargets = new List<int>(64);

    private void OnEnable()
    {
        RegisterWithPlayer();
    }

    private void OnDisable()
    {
        if (playerHealthScript != null) playerHealthScript.UnregisterBarrier(this);
    }

    private void RegisterWithPlayer()
    {
        playerHealthScript = GetComponentInParent<PlayerHealth>();
        if (playerHealthScript != null) playerHealthScript.RegisterBarrier(this);
    }

    // 1. 초기화
    public override void Initialize(WeaponDataSO data)
    {
        if (data is BarrierDataSO barrierData)
        {
            currentChargeTimeLimit = barrierData.chargeTime;
            currentKnockbackRadius = barrierData.knockbackRadius;
            currentKnockbackForce = barrierData.knockbackForce;
            explosionEffect = barrierData.explosionEffectPrefab;

            // 배리어 프리팹을 플레이어 자식으로 생성
            if (barrierData.barrierPrefab != null)
            {
                barrierInstance = Instantiate(barrierData.barrierPrefab, transform.position, Quaternion.identity, transform);
                
                // 배리어 스크립트 가져오기 및 설정
                barrierScript = barrierInstance.GetComponent<BarrierShield>();
                if (barrierScript == null) barrierScript = barrierInstance.AddComponent<BarrierShield>();
                
                barrierScript.Setup(this, explosionEffect);
                
                // 시작은 비활성화
                barrierInstance.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("잘못된 데이터! BarrierDataSO가 필요합니다.");
        }

        RegisterWithPlayer();

        chargeTimer = 0f;
        isBarrierActive = false;

        ActivateBarrier();
    }

    // 2. 매 프레임 실행 (시간 체크)
    public override void OnUpdate()
    {
        // 배리어가 없을 때만 시간을 잰다
        if (!isBarrierActive)
        {
            chargeTimer += Time.deltaTime;

            // 충전 완료 시 배리어 생성
            if (chargeTimer >= GetCooldown(currentChargeTimeLimit))
            {
                ActivateBarrier();
            }
        }
    }

    // 배리어 활성화
    private void ActivateBarrier()
    {
        isBarrierActive = true;
        chargeTimer = 0f;
        if (barrierInstance != null)
        {
            barrierInstance.SetActive(true);
            // 재생성 시 애니메이션 등을 위해 재설정 가능
        }
    }
    public bool TryBlockDamage()
    {
        if (!isActiveAndEnabled || !isBarrierActive)
            return false;

        BreakBarrier();

        return true;
    }

    // 배리어가 맞았을 때 호출됨 (BarrierShield 스크립트에서 호출)
    public void BreakBarrier()
    {
        // 1. 배리어 해제
        isBarrierActive = false;
        chargeTimer = 0f; // 타이머 리셋
        if (barrierInstance != null) barrierInstance.SetActive(false);

        if (playerHealthScript != null)
        {
            playerHealthScript.SetInvincible(0.2f);
        }
        DevLog.Log("베리어!");

        // 2. 넉백 발생 및 데미지 무효화 처리
        PerformKnockback();

        // 3. 이펙트 재생
        barrierScript.PlayExplosion();
    }

    // 주변 적 밀어내기
    private void PerformKnockback()
    {
        var swarm = EnemySwarmSystem.Instance;
        if (swarm == null) return;

        Vector2 origin = transform.position;
        swarm.GetEnemiesInRadius(
            origin, currentKnockbackRadius, knockbackTargets);

        foreach (int index in knockbackTargets)
        {
            Enemy enemy = swarm.TryGetTarget(index);
            if (enemy == null) continue;

            enemy.ApplyKnockback(
                origin, currentKnockbackForce, 0.2f);
        }
    }

    // 외부(PlayerHealth)에서 플레이어가 피격되었음을 알릴 때 호출하는 함수
    // 배리어가 없는 상태에서 맞으면 타이머를 리셋해야 하므로 필요함.
    public void OnPlayerDamaged()
    {
        if (!isBarrierActive)
        {
            chargeTimer = 0f; // 맞으면 충전 초기화
        }
    }
    
    protected override void RecalculateLevelStats()
    {
        if (!(myData is BarrierDataSO data)) return;
        int upgrades = CurrentLevel - 1;
        currentChargeTimeLimit = upgrades == 0 ? data.chargeTime : Mathf.Max(1f, data.chargeTime - 0.5f * upgrades);
        currentKnockbackForce = data.knockbackForce + 2f * upgrades;
        DevLog.Log($"[Barrier Level Up] 대기시간: {currentChargeTimeLimit}, 넉백파워: {currentKnockbackForce}");
    }
}
