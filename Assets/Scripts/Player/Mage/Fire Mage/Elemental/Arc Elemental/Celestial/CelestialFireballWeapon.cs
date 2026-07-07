using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialFireballWeapon : WeaponBase
{
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

    private int chains;
    private float chainRange;


    // [내부 변수]
    private GameObject fireballPrefab;
    private GameObject trailPrefab;
    private GameObject LastFireBoomPrefab;

    private float cooldownTimer;
    private float volleyTimer;
    private int pendingBullets = 0;
    private Enemy currentTarget;

    public override void Initialize(WeaponDataSO data)
    {
        if (data is CelestialFireballSO evoData)
        {
            bulletNumber = evoData.bulletNumber;
            fireRate = evoData.baseCooldown;
            range = evoData.range;
            projectileSpeed = evoData.projectileSpeed;
            currentDamage = evoData.baseDamage;

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
            attackMotion = evoData.attackMotion;

            this.WeaponID = evoData.WeaponId;
        }
        else
        {
            Debug.LogError("잘못된 데이터! CelestialFireballDataSO가 필요합니다.");
        }
        cooldownTimer = 0f;
    }

    public override void OnUpdate()
    {
        if (pendingBullets <= 0)
        {
            if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                currentTarget = EnemySwarmSystem.Instance.GetClosestEnemy(transform.position, range);
                if (currentTarget != null)
                {
                    pendingBullets = bulletNumber;
                    volleyTimer = 0f;
                }
            }
        }
        else
        {
            volleyTimer -= Time.deltaTime;
            if (volleyTimer <= 0f)
            {
                FireProjectile(currentTarget);
            }
        }
    }

    void FireProjectile(Enemy target)
    {
        if (target == null || target.currentNormalState == Enemy.EnemyState.Dead)
        {
            currentTarget = EnemySwarmSystem.Instance.GetClosestEnemy(transform.position, range);
            if (currentTarget == null) 
            {
                pendingBullets = 0;
                cooldownTimer = fireRate;
                return;
            }
            target = currentTarget;
        }

        GameObject fireball = ObjectPoolingManager.Instance.spawnGameObject(fireballPrefab, transform.position, Quaternion.identity);

        if (fireball != null && fireball.TryGetComponent<CelestialFireballProjectile>(out var evoScript))
        {
            float directDamage = GetDamage();
            float finalTrailDamage = directDamage * trailDamageScaling;
            float boomDamage = GetFireBoomDamage();

            evoScript.Setup(
                target.transform, projectileSpeed, trailPrefab, finalTrailDamage, trailDuration, trailSpawnDistance, 
                LastFireBoomPrefab, boomDamage, lastFireBoomSize, chains, chainRange , WeaponID
            );
            
            evoScript.damage = (long)directDamage; // SkillProjectileBase 직격 데미지
        }

        pendingBullets--;
        if (pendingBullets > 0) volleyTimer = 0.1f;
        else cooldownTimer = fireRate;
    }

    // 데미지 계산식
    public float GetDamage()
    {
        float bonus = (PlayerStats.Instance != null) ? PlayerStats.Instance.DamageBonus : 0;
        return currentDamage + bonus;
    }

    public float GetFireBoomDamage()
    {
        float bonus = (PlayerStats.Instance != null) ? PlayerStats.Instance.DamageBonus : 0;
        return currentDamage + (bonus * 2.0f); // 폭발 2배율 적용
    }
    public override void LevelUp()
    {
        currentDamage += 5;
        trailDamageScaling += 0.1f; // 레벨업 시 장판 데미지 비율도 증가
        Debug.Log($"[Evo Fireball Level Up] 직격뎀: {currentDamage}, 장판계수: {trailDamageScaling}");
    }
}
