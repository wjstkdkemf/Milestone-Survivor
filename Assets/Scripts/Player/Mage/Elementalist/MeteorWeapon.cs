using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class MeteorWeapon : WeaponBase
{
    // [런타임 상태 변수]
    private int currentMeteorNumber;
    private float currentCooldownTime;
    private float warningDuration = 1f;
    private float volleyDelay = 0.3f; 
    private float currentScaling;
    private float currentBaseDamage;
    private float searchRadius;
    private float densityCheckRadius;

    // [내부 변수]
    private GameObject MeteorPrefab;
    private GameObject magicCirclePrefab;
    private LayerMask enemyLayerMask;
    private float targetUpdateRate;
    
    // 타이머 변수들
    private float cooldownTimer;       // 공격 쿨타임 계산용
    private float targetUpdateTimer;   // 타겟 검색 최적화용
    private PlayerStats playerStats;   // 데미지 계산용

    // 1. 초기화 (데이터 주입)
    public override void Initialize(WeaponDataSO data)
    {
        if (data is MeteorWeaponSO MeteorData)
        {
            // 데이터로부터 초기값 설정
            currentMeteorNumber = MeteorData.MeteorNumber;
            currentCooldownTime = MeteorData.baseCooldown;
            cooldownTimer = currentCooldownTime;
            warningDuration = MeteorData.warningDuration;
            volleyDelay = MeteorData.volleyDelay;
            searchRadius = MeteorData.range;
            densityCheckRadius = MeteorData.densityCheckRadius;
            currentScaling = MeteorData.playerDamageScaling;
            currentBaseDamage = MeteorData.baseDamage; // 부모 SO의 데미지
            
            MeteorPrefab = MeteorData.MeteorPrefab;
            magicCirclePrefab = MeteorData.magicCirclePrefab;
            enemyLayerMask = MeteorData.enemyLayerMask;
            targetUpdateRate = MeteorData.targetUpdateRate;

            Debug.Log("MeteorWeaponDataSO 완료");
        }
        else
        {
            Debug.LogError("잘못된 데이터! MeteorWeaponDataSO가 필요합니다.");
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

        if (cooldownTimer <= 0f)
        {
            ActivateMeteorStrike();
        }
    }
    void ActivateMeteorStrike()
    {
        // Find all enemies in the search radius first
        Collider2D[] enemiesInSearchRadius = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayerMask);

        
        if (enemiesInSearchRadius.Length == 0)
        {
            return; // No enemies, don't activate the skill
        }

        cooldownTimer = currentCooldownTime;
        
        // Determine all target points for the volley
        List<Vector3> targetPoints = FindTargetPoints(enemiesInSearchRadius);

        // Launch the volley
        StartCoroutine(LaunchMeteorVolley(targetPoints));
    }

    private List<Vector3> FindTargetPoints(Collider2D[] enemies)
    {
        List<Vector3> targets = new List<Vector3>();
        List<Collider2D> potentialRandomTargets = new List<Collider2D>(enemies);

        // 1. Find the densest point for the first meteor
        Vector3? densestPoint = FindDensestPoint(potentialRandomTargets.ToArray());
        if (densestPoint.HasValue)
        {
            targets.Add(densestPoint.Value);
        }
        else
        {
            // Fallback if no point is found, which is unlikely if enemies exist
            targets.Add(potentialRandomTargets[0].transform.position);
        }

        // 2. Find additional random targets for the rest of the meteors
        for (int i = 1; i < currentMeteorNumber; i++)
        {
            if (potentialRandomTargets.Count > 0)
            {
                int randomIndex = Random.Range(0, potentialRandomTargets.Count);
                targets.Add(potentialRandomTargets[randomIndex].transform.position);
                potentialRandomTargets.RemoveAt(randomIndex); // Avoid targeting the same enemy twice
            }
            else
            {
                // If we run out of unique enemies, just target the main densest point again
                targets.Add(densestPoint.Value);
            }
        }

        return targets;
    }
    private Vector3? FindDensestPoint(Collider2D[] enemies)
    {
        if (enemies.Length == 0) return null;

        int maxDensity = 0;
        Vector3 densestPoint = Vector3.zero;

        foreach (Collider2D enemyCollider in enemies)
        {
            Collider2D[] enemiesInDensityRadius = Physics2D.OverlapCircleAll(enemyCollider.transform.position, densityCheckRadius, enemyLayerMask);
            if (enemiesInDensityRadius.Length > maxDensity)
            {
                maxDensity = enemiesInDensityRadius.Length;
                densestPoint = enemyCollider.transform.position;
            }
        }

        return maxDensity > 0 ? densestPoint : (Vector3?)enemies[0].transform.position;
    }

    private IEnumerator LaunchMeteorVolley(List<Vector3> targets)
    {
        foreach (Vector3 target in targets)
        {
            StartCoroutine(MeteorImpactSequence(target));
            yield return new WaitForSeconds(volleyDelay);
        }
    }

    private IEnumerator MeteorImpactSequence(Vector3 targetPoint)
    {
        GameObject circle = Instantiate(magicCirclePrefab, targetPoint, Quaternion.identity);
        yield return new WaitForSeconds(warningDuration);
        Destroy(circle);

        Vector3 meteorSpawnPoint = targetPoint + new Vector3(0, 0, 0);
        GameObject meteor = Instantiate(MeteorPrefab, meteorSpawnPoint, Quaternion.identity);
        if (meteor.TryGetComponent<DoDamage>(out var damageComponent))
        {
            damageComponent.damage = GetDamage();//GetDamage();
        }
    }
    public float GetDamage()
    {
        float bonus = (playerStats != null) ? playerStats.DamageBonus : 0;
        return currentBaseDamage + (bonus * currentScaling);
    }
    public override void LevelUp()
    {
        // 예시: 레벨업 시 총알 개수 증가 혹은 데미지 증가
        currentMeteorNumber++;
        currentBaseDamage += 2f;
        
        Debug.Log($"[Meteor Level Up] 총알: {currentMeteorNumber}, 데미지: {currentBaseDamage}");
    }
}