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
    private PlayerStats playerStats;   // 데미지 계산용

    // 1. 초기화 (데이터 주입)
    public override void Initialize(WeaponDataSO data)
    {
        base.Initialize(data);

        if (data is TurretWeaponDataSO turretData)
        {
            currentBulletNumber = turretData.bulletNumber;
            currentCooldown = turretData.baseCooldown;
            cooldownTimer = currentCooldown;
            currentRange = turretData.range;
            currentScaling = turretData.playerDamageScaling;
            currentDamage = turretData.baseDamage;
            this.WeaponID = turretData.WeaponId;

            attackMotion = turretData.attackMotion;
            
            bulletPrefab = turretData.bulletPrefab;
            targetUpdateRate = turretData.targetUpdateRate;

            Debug.Log("TurretWeaponDataSO 완료");
        }
        else
        {
            Debug.LogError("잘못된 데이터! TurretWeaponDataSO가 필요합니다.");
        }

        if (PlayerStats.Instance != null)
        {
            playerStats = PlayerStats.Instance;
        }
        else
        {
            playerStats = GetComponentInParent<PlayerStats>();
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

        if (closestEnemyPosition != null && cooldownTimer <= 0f)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        // 쿨타임 리셋
        cooldownTimer = currentCooldown;
        float finalDamage = GetDamage();

        // 발사 코루틴 시작
        for (int i = 0; i < currentBulletNumber; i++)
        {
            StartCoroutine(ShootBullet(i * 0.1f , finalDamage));
        }
    }

    // 데미지 계산 함수 (기존 로직 유지)
    public float GetDamage()
    {
        float bonus = (playerStats != null) ? playerStats.DamageBonus : 0;
        return currentDamage + (bonus * currentScaling);
    }

    IEnumerator ShootBullet(float delay , float damage)
    {
        yield return new WaitForSeconds(delay);

        if (closestEnemyPosition != null && closestEnemyPosition.gameObject.activeInHierarchy) 
        {
            GameObject bullet = ObjectPoolingManager.Instance.spawnGameObject(bulletPrefab, transform.position, Quaternion.identity);
            
            if (bullet != null && bullet.TryGetComponent<TurretBullet>(out var bulletSkill))
            {
                bulletSkill.damage = (long)damage;//GetDamage()
                bulletSkill.hitRadius = currentHitRadius;
                bulletSkill.maxHits = currentMaxHits;
                bulletSkill.speed = currentProjectileSpeed;
                
                // 총알 발사!
                bulletSkill.Fire(closestEnemyPosition.position, currentProjectileSpeed, WeaponID);
            }
        }
    }

    void UpdateTarget()
    {
        Enemy closestEnemy = EnemySwarmSystem.Instance.GetClosestEnemy(transform.position, currentRange);

        closestEnemyPosition = closestEnemy != null ? closestEnemy.transform : null;
    }

    public override void LevelUp()
    {
        currentBulletNumber++; 
        currentDamage += 2;
        
        Debug.Log($"[Turret Level Up] 총알: {currentBulletNumber}, 데미지: {currentDamage}");
    }

    public override UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = base.GetUpgradePreview(upgrade);

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.amount",
            currentBulletNumber.ToString(),
            (currentBulletNumber + 1).ToString()
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.damage",
            currentDamage.ToString(),
            (currentDamage + 2).ToString()
        ));

        return preview;
    }
}
