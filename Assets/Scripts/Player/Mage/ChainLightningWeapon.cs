using System.Collections.Generic;
using UnityEngine;

public class ChainLightningWeapon : WeaponBase
{
    // [런타임 상태 변수]
    private float currentCooldownTime;
    private float currentInitialRange;
    private float currentProjectileSpeed;
    private int currentChainCount;
    private float currentChainRange;
    private float currentDamageReduction;
    private float currentBaseDamage;
    private float currentScaling;

    // [내부 변수]
    private GameObject projectilePrefab;
    private LayerMask enemyLayerMask;
    private float cooldownTimer;
    private PlayerStats playerStats;

    // 1. 초기화
    public override void Initialize(WeaponDataSO data)
    {
        if (data is ChainLightningDataSO lightningData)
        {
            currentCooldownTime = lightningData.baseCooldown;
            currentInitialRange = lightningData.initialRange;
            currentProjectileSpeed = lightningData.projectileSpeed;
            currentChainCount = lightningData.chainCount;
            currentChainRange = lightningData.chainRange;
            currentDamageReduction = lightningData.damageReductionPerBounce;
            currentBaseDamage = lightningData.baseDamage;
            currentScaling = lightningData.playerDamageScaling;

            projectilePrefab = lightningData.projectilePrefab;
            enemyLayerMask = lightningData.enemyLayerMask;
        }
        else
        {
            Debug.LogError("잘못된 데이터! ChainLightningDataSO가 필요합니다.");
        }

        if (PlayerStats.Instance != null)
             playerStats = PlayerStats.Instance;
        else
             playerStats = GetComponentInParent<PlayerStats>();

        cooldownTimer = 0f; // 시작하자마자 쏠 수 있게 0으로 초기화
    }

    // 2. 매 프레임 실행
    public override void OnUpdate()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (cooldownTimer <= 0f)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        // 가장 가까운 첫 번째 타겟 찾기 (TurretWeapon 로직 재사용)
        Transform closestTarget = FindClosestEnemy(transform.position, currentInitialRange);

        if (closestTarget != null)
        {
            FireFirstLightning(closestTarget);
            cooldownTimer = currentCooldownTime;
        }
    }

    private void FireFirstLightning(Transform target)
    {
        // 첫 발사체 생성
        GameObject lightning = ObjectPoolingManager.instance.spawnGameObject(projectilePrefab, transform.position, Quaternion.identity);

        // 데미지 계산 및 설정 (DoDamage 스크립트)
        float finalDamage = GetDamage();
        if (lightning.TryGetComponent<DoDamage>(out var damageComponent))
        {
            damageComponent.damage = finalDamage;
        }

        // 체인 로직 설정 (새로운 스크립트)
        if (lightning.TryGetComponent<ChainLightningProjectile>(out var chainComponent))
        {
            // 이미 맞은 적을 기록할 리스트 생성 (이번 체인 공격 동안 공유됨)
            HashSet<GameObject> visitedTargets = new HashSet<GameObject>();
            
            // 첫 타겟 정보 주입
            chainComponent.Setup(target, currentProjectileSpeed, currentChainCount, currentChainRange, currentDamageReduction, enemyLayerMask, visitedTargets);
        }
    }

    // 가장 가까운 적 찾는 함수 (재사용성을 위해 분리)
    private Transform FindClosestEnemy(Vector3 center, float range)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, range, enemyLayerMask);
        Transform closestEnemy = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.TryGetComponent<IDamageable>(out _))
            {
                float distanceToEnemySqr = (hit.transform.position - center).sqrMagnitude;
                if (distanceToEnemySqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceToEnemySqr;
                    closestEnemy = hit.transform;
                }
            }
        }
        return closestEnemy;
    }
    
    public float GetDamage()
    {
        float bonus = (playerStats != null) ? playerStats.DamageBonus : 0;
        return currentBaseDamage + (bonus * currentScaling);
    }

    public override void LevelUp()
    {
        // 예시: 레벨업 시 데미지 증가 및 튕기는 횟수 증가
        currentBaseDamage += 3f;
        if (currentChainCount < 5) // 최대 5번까지만 증가
            currentChainCount++;
        Debug.Log($"[Chain Lightning Level Up] 데미지: {currentBaseDamage}, 체인 횟수: {currentChainCount}");
    }
}