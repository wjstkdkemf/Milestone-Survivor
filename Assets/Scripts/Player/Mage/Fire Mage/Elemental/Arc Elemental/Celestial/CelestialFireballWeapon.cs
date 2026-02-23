using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialFireballWeapon : WeaponBase
{
    // [런타임 데이터]
    private int bulletNumber;
    private float fireRate;
    private float range;
    private float projectileSpeed;
    private float lastFireBoomSize;
    private float lastFireBoomDamage;
    // 장판 관련
    private float trailDamageScaling;
    private float trailDuration;
    private float trailSpawnDistance;

    private float currentBaseDamage;
    private float currentPlayerScaling = 1.0f; // 기본값
    private int chains;
    private float chainRange;


    // [내부 변수]
    private GameObject fireballPrefab;
    private GameObject trailPrefab;
    private GameObject LastFireBoomPrefab;
    private LayerMask enemyLayerMask;
    private float cooldownTimer;
    private PlayerStats playerStats;

    public override void Initialize(WeaponDataSO data)
    {
        if (data is CelestialFireballSO evoData)
        {
            bulletNumber = evoData.bulletNumber;
            fireRate = evoData.baseCooldown;
            range = evoData.range;
            projectileSpeed = evoData.projectileSpeed;
            currentBaseDamage = evoData.baseDamage;

            lastFireBoomSize = evoData.FireBoomSize;
            
            // 장판 데이터
            trailDamageScaling = evoData.trailDamageScaling;
            trailDuration = evoData.trailDuration;
            trailSpawnDistance = evoData.trailSpawnDistance;

            chains = evoData.chainCount;
            chainRange = evoData.chainRange;

            fireballPrefab = evoData.fireballPrefab;
            trailPrefab = evoData.trailPrefab;
            LastFireBoomPrefab = evoData.lastFireBoomPrefab;
            enemyLayerMask = evoData.enemyLayerMask;
        }
        else
        {
            Debug.LogError("잘못된 데이터! CelestialFireballDataSO가 필요합니다.");
        }

        if (PlayerStats.Instance != null) playerStats = PlayerStats.Instance;
        else playerStats = GetComponentInParent<PlayerStats>();

        cooldownTimer = 0f;
    }

    public override void OnUpdate()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            Transform target = FindClosestEnemy();
            if (target != null)
            {
                StartCoroutine(FireVolley(target));
                cooldownTimer = fireRate;
            }
        }
    }

    IEnumerator FireVolley(Transform target)
    {
        for (int i = 0; i < bulletNumber; i++)
        {
            FireProjectile(target);
            yield return new WaitForSeconds(0.1f); // 연사 딜레이
        }
    }

    void FireProjectile(Transform target)
    {
        // 투사체 풀링 생성
        GameObject fireball = ObjectPoolingManager.instance.spawnGameObject(fireballPrefab, transform.position, Quaternion.identity);

        float directDamage = GetDamage();
        float finalTrailDamage = directDamage * trailDamageScaling;
        lastFireBoomDamage = GetFireBoomDamage();

        // 2. 이동 및 장판 설정 (새로운 스크립트)
        if (fireball.TryGetComponent<CelestialFireballProjectile>(out var evoScript))
        {
            ElementalFireballSO evoData = myData as ElementalFireballSO;

            // 투사체 셋업에 모든 정보를 넘겨줍니다.
            evoScript.Setup(
                target, projectileSpeed, trailPrefab, finalTrailDamage, trailDuration, trailSpawnDistance, 
                LastFireBoomPrefab, lastFireBoomDamage, lastFireBoomSize, chains, chainRange, enemyLayerMask
            );
        }
    }

    // 데미지 계산식
    public float GetDamage()
    {
        float bonus = (playerStats != null) ? playerStats.DamageBonus : 0;
        return currentBaseDamage + (bonus * currentPlayerScaling);
    }
    public float GetFireBoomDamage()
    {
        float bonus = (playerStats != null) ? playerStats.DamageBonus : 0;
        return currentBaseDamage + (bonus * (currentPlayerScaling + 1.0f));
    }

    // 가장 가까운 적 찾기 (TurretWeapon 재사용)
    private Transform FindClosestEnemy()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, range, enemyLayerMask);
        Transform closest = null;
        float closestDistSqr = Mathf.Infinity;

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent<IDamageable>(out _))
            {
                float distSqr = (hit.transform.position - transform.position).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closest = hit.transform;
                }
            }
        }
        return closest;
    }

    public override void LevelUp()
    {
        currentBaseDamage += 5f;
        trailDamageScaling += 0.1f; // 레벨업 시 장판 데미지 비율도 증가
        Debug.Log($"[Evo Fireball Level Up] 직격뎀: {currentBaseDamage}, 장판계수: {trailDamageScaling}");
    }
}
