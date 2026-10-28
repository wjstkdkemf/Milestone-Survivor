using System.Collections;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class TurretWeapon : WeaponBase
{
    // [런타임 상태 변수]
    private int currentBulletNumber;
    private float currentRange;
    private float currentScaling;

    // [내부 변수]
    private GameObject bulletPrefab;
    private float targetUpdateRate;
    
    // 타이머 변수들
    private float cooldownTimer;       // 공격 쿨타임 계산용
    private float targetUpdateTimer;   // 타겟 검색 최적화용
    private Transform closestEnemyPosition;
    // Keep retries isolated when a new volley overlaps an older one's delayed shots.
    private sealed class VolleyState
    {
        public int PendingShots;
        public bool Started;
    }

    private VolleyState waitingVolley;

    // 1. 초기화 (데이터 주입)
    public override void Initialize(WeaponDataSO data)
    {
        base.Initialize(data);

        if (data is TurretWeaponDataSO turretData)
        {
            currentBulletNumber = turretData.bulletNumber;
            currentCooldown = turretData.baseCooldown;
            cooldownTimer = GetCooldown(currentCooldown);
            currentRange = turretData.range;
            currentScaling = turretData.playerDamageScaling;
            currentDamage = turretData.baseDamage;
            this.WeaponID = turretData.WeaponId;

            attackMotion = turretData.attackMotion;
            
            bulletPrefab = turretData.bulletPrefab;
            targetUpdateRate = turretData.targetUpdateRate;

            DevLog.Log("TurretWeaponDataSO 완료");
        }
        else
        {
            Debug.LogError("잘못된 데이터! TurretWeaponDataSO가 필요합니다.");
        }

    }

    public override void OnUpdate()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        targetUpdateTimer -= Time.deltaTime;
        if (targetUpdateTimer <= 0f)
        {
            UpdateTarget();
            targetUpdateTimer = targetUpdateRate;
        }

        if (closestEnemyPosition != null && cooldownTimer <= 0f && waitingVolley == null)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (currentBulletNumber <= 0) return;

        var volley = new VolleyState { PendingShots = currentBulletNumber };
        waitingVolley = volley;
        long finalDamage = GetDamage();

        // 발사 코루틴 시작
        for (int i = 0; i < currentBulletNumber; i++)
        {
            StartCoroutine(ShootBullet(i * 0.1f , finalDamage, volley));
        }
    }

    // 데미지 계산 함수 (기존 로직 유지)
    public long GetDamage()
    {
        return CalculateDamage(currentDamage, currentScaling);
    }

    IEnumerator ShootBullet(float delay , long damage, VolleyState volley)
    {
        yield return new WaitForSeconds(delay);

        while (closestEnemyPosition != null && closestEnemyPosition.gameObject.activeInHierarchy)
        {
            if (Time.timeScale <= 0f || !TryShootBullet(damage))
            {
                yield return null;
                continue;
            }

            if (!volley.Started)
            {
                cooldownTimer = GetCooldown(currentCooldown);
                volley.Started = true;
                waitingVolley = null;
            }
            break;
        }

        volley.PendingShots--;
        if (!volley.Started && volley.PendingShots == 0)
            waitingVolley = null;
    }

    private bool TryShootBullet(long damage)
    {
        var pool = ObjectPoolingManager.Instance;
        if (pool == null) return false;

        GameObject bullet = pool.spawnGameObject(bulletPrefab, transform.position, Quaternion.identity);
        if (bullet == null) return false;
        if (!bullet.TryGetComponent<TurretBullet>(out var bulletSkill))
        {
            pool.ReturnObjectToPool(bullet);
            return false;
        }

        bulletSkill.damage = damage;
        bulletSkill.hitRadius = currentHitRadius;
        bulletSkill.maxHits = currentMaxHits;
        bulletSkill.speed = currentProjectileSpeed;
        bulletSkill.Fire(closestEnemyPosition.position, currentProjectileSpeed, WeaponID);
        if (weaponVisual != null) weaponVisual.SetTarget(closestEnemyPosition);
        PlayAttackMotion();
        return true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        waitingVolley = null;
    }

    void UpdateTarget()
    {
        Enemy closestEnemy = EnemySwarmSystem.Instance.GetClosestEnemy(transform.position, currentRange);

        closestEnemyPosition = closestEnemy != null ? closestEnemy.transform : null;
    }

    protected override void RecalculateLevelStats()
    {
        if (!(myData is TurretWeaponDataSO data)) return;
        currentBulletNumber = CalculateCount(SkillStatType.ProjectileCount, data.bulletNumber, CurrentLevel);
        currentDamage = CalculateLevelDamage(data.baseDamage, CurrentLevel);
        
        DevLog.Log($"[Turret Level Up] 총알: {currentBulletNumber}, 데미지: {currentDamage}");
    }

    public override UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = base.GetUpgradePreview(upgrade);
        if (!(myData is TurretWeaponDataSO data)) return preview;

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.amount",
            currentBulletNumber.ToString(),
            CalculateCount(SkillStatType.ProjectileCount, data.bulletNumber, CurrentLevel + 1).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.damage",
            currentDamage.ToString(),
            CalculateLevelDamage(data.baseDamage, CurrentLevel + 1).ToString()
        ));

        return preview;
    }
}
