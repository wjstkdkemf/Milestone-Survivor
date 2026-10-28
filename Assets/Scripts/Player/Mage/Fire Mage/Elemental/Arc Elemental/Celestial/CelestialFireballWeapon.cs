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
                cooldownTimer = GetCooldown(fireRate);
                return;
            }
            target = currentTarget;
        }

        var pool = ObjectPoolingManager.Instance;
        if (pool == null) return;

        GameObject fireball = pool.spawnGameObject(fireballPrefab, transform.position, Quaternion.identity);
        if (fireball == null) return;
        if (!fireball.TryGetComponent<CelestialFireballProjectile>(out var evoScript))
        {
            pool.ReturnObjectToPool(fireball);
            return;
        }

        long directDamage = GetDamage();
        float finalTrailDamage = directDamage * trailDamageScaling;
        float boomDamage = GetFireBoomDamage();
        evoScript.Setup(
            target.transform, projectileSpeed, trailPrefab, finalTrailDamage, trailDuration, trailSpawnDistance,
            LastFireBoomPrefab, boomDamage, lastFireBoomSize, chains, chainRange , WeaponID
        );
        evoScript.damage = directDamage; // SkillProjectileBase 직격 데미지

        pendingBullets--;
        if (pendingBullets > 0) volleyTimer = 0.1f;
        else cooldownTimer = GetCooldown(fireRate);
    }

    // 데미지 계산식
    public long GetDamage()
    {
        return CalculateDamage(currentDamage);
    }

    public long GetFireBoomDamage()
    {
        return CalculateDamage(currentDamage, 2d);
    }
    protected override void RecalculateLevelStats()
    {
        if (!(myData is CelestialFireballSO data)) return;
        currentDamage = CalculateLevelDamage(data.baseDamage, CurrentLevel);
        trailDamageScaling = CalculateLevelStat(SkillStatType.TrailDamageScaling, data.trailDamageScaling, CurrentLevel);
        DevLog.Log($"[Evo Fireball Level Up] 직격뎀: {currentDamage}, 장판계수: {trailDamageScaling}");
    }
}
