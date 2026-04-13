using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialFireballProjectile : SkillProjectileBase
{
    private Transform target;
    private float speed;
    
    // 장판 관련 데이터
    private GameObject trailPrefab;
    private float trailDamage;
    private float trailDuration;
    private float spawnDistanceThreshold;
    private GameObject lastFireBoom;
    private float lastFireBoomDamage;
    private float lastFireBoomSize; // 폭발 크기

    // 체인(튕기기) 관련 데이터
    private int remainingChains;
    private float chainRange;
    private HashSet<Enemy> visitedTargets = new HashSet<Enemy>();

    private Vector3 lastSpawnPosition;
    private int state = 0;
    private float bounceTimer = 0f;

    // 레이더 바구니
    private List<int> searchResults = new List<int>(50);

    // Setup에 체인 관련 변수 3개(chains, cRange, layerMask)와 폭발 크기(boomSize) 추가
    public void Setup(Transform newTarget, float newSpeed, GameObject trail, float tDamage, float tDuration, float tSpawnDist, 
                      GameObject FireBoomPrefab, float FireBoomDamage, float boomSize, int chains, float cRange)
    {
        target = newTarget;
        speed = newSpeed;
        hitRadius = 0.5f;
        maxHits = 999;

        trailPrefab = trail;
        trailDamage = tDamage;
        trailDuration = tDuration;
        spawnDistanceThreshold = tSpawnDist;
        lastFireBoom = FireBoomPrefab;
        lastFireBoomDamage = FireBoomDamage;
        lastFireBoomSize = boomSize;
    
        remainingChains = chains;
        chainRange = cRange;
        
        visitedTargets.Clear();
        state = 0;

        lastSpawnPosition = transform.position;

        if (target != null) RotateTowardsTarget(target.position);
    }

    void Update()
    {
        if (state == 1)
        {
            bounceTimer -= Time.deltaTime;
            if (bounceTimer <= 0f)
            {
                FindNextTargetAndResume();
            }
            return; // 대기 중에는 이동하지 않음
        }

        Vector3 moveDirection = transform.right; 
        if (target != null && target.gameObject.activeInHierarchy)
        {
            moveDirection = (target.position - transform.position).normalized;
            RotateTowardsTarget(target.position);
        }

        transform.position += moveDirection * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, lastSpawnPosition) >= spawnDistanceThreshold)
        {
            SpawnTrail();
            lastSpawnPosition = transform.position; 
        }
    }

    void SpawnTrail()
    {
        if (trailPrefab == null) return;
        GameObject trail = ObjectPoolingManager.Instance.spawnGameObject(trailPrefab, transform.position, Quaternion.identity);
        
        if (trail != null && trail.TryGetComponent<AuraZone>(out var trailSkill))
        {
            trailSkill.SetupAura(0.5f, trailDamage, 1.5f, false, 0f, trailDuration); 
        }
    }

    public override void OnHit(Enemy hitEnemy)
    {
        if (state == 1 || visitedTargets.Contains(hitEnemy)) return;

        visitedTargets.Add(hitEnemy);

        if (lastFireBoom != null)
        {
            ObjectPoolingManager.Instance.spawnGameObject(lastFireBoom, transform.position, Quaternion.identity);
        }

        EnemySwarmSystem.Instance.GetEnemiesInRadius(transform.position, lastFireBoomSize, searchResults);
        for (int i = 0; i < searchResults.Count; i++)
        {
            int idx = searchResults[i];
            if (idx >= EnemySwarmSystem.Instance.activeEnemies.Count) continue;
            
            Enemy boomTarget = EnemySwarmSystem.Instance.activeEnemies[idx];
            if (boomTarget == null || boomTarget.currentNormalState == Enemy.EnemyState.Dead) continue;

            if (Time.time >= EnemySwarmSystem.Instance.nextHitTimes[idx])
            {
                boomTarget.TakeDamage(lastFireBoomDamage);
                EnemySwarmSystem.Instance.nextHitTimes[idx] = Time.time + 0.1f;
            }
        }

        remainingChains--;
        if (remainingChains > 0)
        {
            state = 1;
            bounceTimer = 0.5f;
        }
        else
        {
            ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
        }
    }
    private void FindNextTargetAndResume()
    {
        EnemySwarmSystem.Instance.GetEnemiesInRadius(transform.position, chainRange, searchResults);
        
        Enemy bestTarget = null;
        float closestDistSqr = Mathf.Infinity;

        for (int i = 0; i < searchResults.Count; i++)
        {
            int idx = searchResults[i];
            if (idx >= EnemySwarmSystem.Instance.activeEnemies.Count) continue;

            Enemy e = EnemySwarmSystem.Instance.activeEnemies[idx];
            
            // 이미 맞춘 적이거나 죽은 적은 패스
            if (e == null || e.currentNormalState == Enemy.EnemyState.Dead || visitedTargets.Contains(e)) continue;

            float distSqr = (e.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                bestTarget = e;
            }
        }

        if (bestTarget != null)
        {
            target = bestTarget.transform;
            RotateTowardsTarget(target.position);
            lastSpawnPosition = transform.position; // 궤적 기준점 리셋
            state = 0; // 다시 비행 시작!
        }
        else
        {
            ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject); // 주변에 적이 없으면 소멸
        }
    }
    
    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
