using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalFireballProjectile : SkillProjectileBase
{
    private Transform target;
    
    // 장판 관련 데이터
    private GameObject trailPrefab;
    private float trailDamage;
    private float trailDuration;
    private float spawnDistanceThreshold;
    private Vector3 lastSpawnPosition; // 마지막으로 장판을 깐 위치

    private GameObject lastFireBoom;
    private float fireBoomDamage;
    private float fireBoomSize;

    private List<int> enemiesHitIndices = new List<int>(50);
    private float speed;

    // 무기에서 호출하여 데이터 초기화
    public void Setup(Transform newTarget, float newSpeed, GameObject trail, float tDamage, float tDuration, float tSpawnDist, 
                      GameObject boomPrefab, float boomDamage, float boomSize)
    {
        target = newTarget;
        speed = newSpeed;
        hitRadius = 0.5f;
        maxHits = 1;
       
        trailPrefab = trail;
        trailDamage = tDamage;
        trailDuration = tDuration;
        spawnDistanceThreshold = tSpawnDist;
    
        lastFireBoom = boomPrefab;
        fireBoomDamage = boomDamage;
        fireBoomSize = boomSize;

        lastSpawnPosition = transform.position; 

        if (target != null) RotateTowardsTarget(target.position);
    }
    void Update()
    {
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

        // 🚨 장판 프리팹은 반드시 Collider 없이 AuraSkill(혹은 ZoneDamageArea)이 붙어있어야 합니다!
        if (trail != null && trail.TryGetComponent<AuraZone>(out var trailSkill))
        {
            trailSkill.SetupAura(0.5f, trailDamage, 1.5f, false, 0f, trailDuration); 
        }
    }
    public override void OnHit(Enemy hitEnemy)
    {
        if (lastFireBoom != null)
        {
            ObjectPoolingManager.Instance.spawnGameObject(lastFireBoom, transform.position, Quaternion.identity);
        }

        EnemySwarmSystem.Instance.GetEnemiesInRadius(transform.position, fireBoomSize, enemiesHitIndices);

        for (int i = 0; i < enemiesHitIndices.Count; i++)
        {
            int idx = enemiesHitIndices[i];
            
            if (idx >= EnemySwarmSystem.Instance.activeEnemies.Count) continue;
            Enemy target = EnemySwarmSystem.Instance.activeEnemies[idx];
            if (target == null || target.currentNormalState == Enemy.EnemyState.Dead) continue;

            if (Time.time >= EnemySwarmSystem.Instance.nextHitTimes[idx])
            {
                target.TakeDamage(fireBoomDamage);
                EnemySwarmSystem.Instance.nextHitTimes[idx] = Time.time + 0.1f;
            }
        }

        ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
    }
    
    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
