using System.Collections;
using UnityEngine;

public class TurretWeapon : WeaponBase
{
    // [런타임 상태 변수]
    private int currentBulletNumber;
    private float currentCooldownTime;
    private float currentRange;
    private float currentScaling;
    private float currentBaseDamage;

    // [내부 변수]
    private GameObject bulletPrefab;
    private LayerMask enemyLayerMask;
    private float targetUpdateRate;
    
    // 타이머 변수들
    private float cooldownTimer;       // 공격 쿨타임 계산용
    private float targetUpdateTimer;   // 타겟 검색 최적화용
    private Transform closestEnemyPosition;
    private PlayerStats playerStats;   // 데미지 계산용

    // 1. 초기화 (데이터 주입)
    public override void Initialize(WeaponDataSO data)
    {
        if (data is TurretWeaponDataSO turretData)
        {
            // 데이터로부터 초기값 설정
            currentBulletNumber = turretData.bulletNumber;
            currentCooldownTime = turretData.baseCooldown;
            cooldownTimer = currentCooldownTime;
            currentRange = turretData.range;
            currentScaling = turretData.playerDamageScaling;
            currentBaseDamage = turretData.baseDamage; // 부모 SO의 데미지
            
            bulletPrefab = turretData.bulletPrefab;
            enemyLayerMask = turretData.enemyLayerMask;
            targetUpdateRate = turretData.targetUpdateRate;

            Debug.Log("TurretWeaponDataSO 완료");
        }
        else
        {
            Debug.LogError("잘못된 데이터! TurretWeaponDataSO가 필요합니다.");
        }

        // 플레이어 스탯 가져오기 (싱글톤 혹은 부모 컴포넌트)
        if (PlayerStats.Instance != null)
        {
            playerStats = PlayerStats.Instance;
        }
        else
        {
            // 만약 싱글톤이 아니라면 부모에서 찾기
            playerStats = GetComponentInParent<PlayerStats>();
        }
    }

    // 2. 매 프레임 실행 (PlayerWeaponController가 호출)
    public override void OnUpdate()
    {
        // 쿨타임 감소
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 타겟 검색 (최적화를 위해 일정 시간마다 실행)
        targetUpdateTimer -= Time.deltaTime;
        if (targetUpdateTimer <= 0f)
        {
            UpdateTarget();
            targetUpdateTimer = targetUpdateRate;
        }

        // 공격 조건: 타겟이 있고 + 쿨타임이 돌았을 때
        if (closestEnemyPosition != null && cooldownTimer <= 0f)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        // 쿨타임 리셋
        cooldownTimer = currentCooldownTime;

        // 발사 코루틴 시작
        for (int i = 0; i < currentBulletNumber; i++)
        {
            StartCoroutine(ShootBullet(i * 0.1f));
        }
    }

    // 데미지 계산 함수 (기존 로직 유지)
    public float GetDamage()
    {
        float bonus = (playerStats != null) ? playerStats.DamageBonus : 0;
        return currentBaseDamage + (bonus * currentScaling);
    }

    IEnumerator ShootBullet(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 오브젝트 풀링 사용
        if (closestEnemyPosition != null) // 쏘는 순간 타겟이 사라졌을 수도 있으니 체크
        {
            GameObject bullet = ObjectPoolingManager.Instance.spawnGameObject(bulletPrefab, transform.position, Quaternion.identity);
            
            if (bullet == null)
            {
                // 총알이 없는데 설정을 시도하면 에러가 나므로, 여기서 중단합니다.
                yield break; 
            }
            // 총알 설정
            if (bullet.TryGetComponent<TurretBullet>(out var turretBullet))
            {
                turretBullet.EnemyPosition = closestEnemyPosition;
            }

            // 데미지 설정
            if (bullet.TryGetComponent<DoDamage>(out var damageComponent))
            {
                damageComponent.damage = GetDamage();//GetDamage();
            }
        }
    }

    void UpdateTarget()
    {
        // 기존 타겟팅 로직 그대로 사용
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, currentRange, enemyLayerMask);
        
        Transform closestEnemyTransform = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Collider2D hit in hitColliders)
        {
            // IDamageable 인터페이스 확인 (기존 로직)
            if (hit.TryGetComponent<IDamageable>(out _))
            {
                float distanceToEnemySqr = (hit.transform.position - transform.position).sqrMagnitude;

                if (distanceToEnemySqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceToEnemySqr;
                    closestEnemyTransform = hit.transform;
                }
            }
        }
        closestEnemyPosition = closestEnemyTransform;
    }

    //레벨업 로직 (UpgradeManager에서 호출됨)
    public override void LevelUp()
    {
        // 예시: 레벨업 시 총알 개수 증가 혹은 데미지 증가
        currentBulletNumber++; 
        currentBaseDamage += 2f;
        
        Debug.Log($"[Turret Level Up] 총알: {currentBulletNumber}, 데미지: {currentBaseDamage}");
    }
}