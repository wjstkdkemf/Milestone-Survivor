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
    private LayerMask enemyLayerMask;
    private LayerMask projectileLayerMask;

    private PlayerHealth playerHealthScript;

    // 1. 초기화
    public override void Initialize(WeaponDataSO data)
    {
        if (data is BarrierDataSO barrierData)
        {
            currentChargeTimeLimit = barrierData.chargeTime;
            currentKnockbackRadius = barrierData.knockbackRadius;
            currentKnockbackForce = barrierData.knockbackForce;
            explosionEffect = barrierData.explosionEffectPrefab;
            enemyLayerMask = barrierData.enemyLayerMask;
            projectileLayerMask = barrierData.projectileLayerMask;

            // 배리어 프리팹을 플레이어 자식으로 생성
            if (barrierData.barrierPrefab != null)
            {
                barrierInstance = Instantiate(barrierData.barrierPrefab, transform.position, Quaternion.identity, transform);
                
                // 배리어 스크립트 가져오기 및 설정
                barrierScript = barrierInstance.GetComponent<BarrierShield>();
                if (barrierScript == null) barrierScript = barrierInstance.AddComponent<BarrierShield>();
                
                barrierScript.Setup(this, enemyLayerMask, projectileLayerMask);
                
                // 시작은 비활성화
                barrierInstance.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("잘못된 데이터! BarrierDataSO가 필요합니다.");
        }

        playerHealthScript = GetComponentInParent<PlayerHealth>();

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
            if (chargeTimer >= currentChargeTimeLimit)
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

    // 배리어가 맞았을 때 호출됨 (BarrierShield 스크립트에서 호출)
    public void BreakBarrier()
    {
        if (!isBarrierActive) return;

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
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
    }

    // 주변 적 밀어내기
    private void PerformKnockback()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, currentKnockbackRadius, enemyLayerMask);

        foreach (Collider2D col in colliders)
        {
            Enemy enemyScript = col.GetComponent<Enemy>();
            
            if (enemyScript != null && enemyScript.CantBeKnocked)
            {
                continue;
            }
            // 적에게 Rigidbody2D가 있다면 힘을 가함
            Rigidbody2D enemyRb = col.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 direction = (col.transform.position - transform.position).normalized;
                // ForceMode2D.Impulse로 순간적인 힘을 가함
                enemyRb.AddForce(direction * currentKnockbackForce, ForceMode2D.Impulse);
            }
            
            // (선택사항) 만약 적이 투사체라면 파괴할 수도 있음
            //if (col.CompareTag("EnemyProjectile")) Destroy(col.gameObject);
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
    
    public override void LevelUp()
    {
        // 레벨업 시 쿨타임 감소, 넉백 파워 증가 등
        currentChargeTimeLimit = Mathf.Max(1f, currentChargeTimeLimit - 0.5f);
        currentKnockbackForce += 2f;
        DevLog.Log($"[Barrier Level Up] 대기시간: {currentChargeTimeLimit}, 넉백파워: {currentKnockbackForce}");
    }
}
