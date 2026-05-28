using System.Collections;
using UnityEngine;

public class EvolvedFireballWeapon : WeaponBase
{
    private int bulletNumber;
    private float fireRate;
    private float range;
    private float projectileSpeed;
    // 장판 관련
    private float trailDamageScaling;
    private float trailDuration;
    private float trailSpawnDistance;
    // [내부 변수]
    private GameObject fireballPrefab;
    private GameObject trailPrefab;

    private float cooldownTimer;
    private float volleyTimer;
    private int pendingBullets = 0;
    private Enemy currentTarget;

    public override void Initialize(WeaponDataSO data)
    {
        if (data is EvolvedFireballDataSO evoData)
        {
            bulletNumber = evoData.bulletNumber;
            fireRate = evoData.baseCooldown;
            range = evoData.range;
            projectileSpeed = evoData.projectileSpeed;
            currentDamage = evoData.baseDamage;
            
            // 장판 데이터
            trailDamageScaling = evoData.trailDamageScaling;
            trailDuration = evoData.trailDuration;
            trailSpawnDistance = evoData.trailSpawnDistance;

            fireballPrefab = evoData.fireballPrefab;
            trailPrefab = evoData.trailPrefab;

            this.WeaponID = evoData.WeaponId;
        }
        else
        {
            Debug.LogError("잘못된 데이터! EvolvedFireballDataSO가 필요합니다.");
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
                // 인덱스 레이더로 단일 타겟 서치!
                currentTarget = EnemySwarmSystem.Instance.GetClosestEnemy(transform.position, range);
                
                if (currentTarget != null)
                {
                    pendingBullets = bulletNumber;
                    volleyTimer = 0f;
                }
            }
        }
        else // 연사(Volley) 중
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

        if (fireball != null && fireball.TryGetComponent<EvolvedFireballProjectile>(out var evoScript))
        {
            float directDamage = GetDamage();
            float finalTrailDamage = directDamage * trailDamageScaling;
            
            evoScript.SetupEvo(target.transform, projectileSpeed, trailPrefab, (long)directDamage, finalTrailDamage, trailDuration, trailSpawnDistance , WeaponID);
        }

        pendingBullets--;

        if (pendingBullets > 0)
        {
            volleyTimer = 0.1f; // 연사 딜레이
        }
        else
        {
            cooldownTimer = fireRate;
        }
    }

    // 데미지 계산식
    public float GetDamage()
    {
        float bonus = (PlayerStats.Instance != null) ? PlayerStats.Instance.DamageBonus : 0;
        return currentDamage + bonus; 
    }

    public override void LevelUp()
    {
        currentDamage += 5;
        trailDamageScaling += 0.1f; 
    }
}