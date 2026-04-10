using UnityEngine;
using System.Collections.Generic;

public class Meteor : SkillProjectileBase
{
    [Header("Impact")]
    public float impactRadius = 4f; 
    public float fallSpeed = 20f;
    public float warningDuration = 1f;

    [Header("Effects")]
    public GameObject explosionPrefab;
    public GameObject magicCirclePrefab;
    private Vector3 targetPosition;
    private GameObject activeMagicCircle;

    private int state = 0; 
    private float timer = 0f;
    private List<int> enemiesHitIndices = new List<int>(100);

    public void Fire(Vector3 targetPos, float meteorDamage, float expRadius)
    {
        this.damage = meteorDamage;
        this.impactRadius = expRadius;
        
        this.hitRadius = 0f; 
        this.maxHits = 0;    

        this.targetPosition = targetPos;
        
        transform.position = targetPosition + new Vector3(0, 15f, 0);

        if (magicCirclePrefab != null)
        {
            activeMagicCircle = ObjectPoolingManager.Instance.spawnGameObject(magicCirclePrefab, targetPosition, Quaternion.identity);
        }
        

        state = 0;
        timer = warningDuration;
    }

    // 부모의 이동 로직(Update)을 메테오 전용 낙하 로직으로 덮어씌웁니다.
    protected virtual void Update()
    {
        if (state == 0) // 경고 단계
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                if (activeMagicCircle != null) ObjectPoolingManager.Instance.ReturnObjectToPool(activeMagicCircle);
                state = 1; // 낙하 시작
            }
        }
        else if (state == 1) // 낙하 단계
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

            // 땅(목표 좌표)에 닿으면 폭발!
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                Explode();
            }
        }
    }

    void Explode()
    {
        if (explosionPrefab != null)
        {
            ObjectPoolingManager.Instance.spawnGameObject(explosionPrefab, targetPosition, Quaternion.identity);
        }

        EnemySwarmSystem.Instance.GetEnemiesInRadius(targetPosition, impactRadius, enemiesHitIndices);

        for (int i = 0; i < enemiesHitIndices.Count; i++)
        {
            int idx = enemiesHitIndices[i];
            
            if (idx >= EnemySwarmSystem.Instance.activeEnemies.Count) continue;
            Enemy target = EnemySwarmSystem.Instance.activeEnemies[idx];
            if (target == null || target.currentNormalState == Enemy.EnemyState.Dead) continue;

            if (Time.time >= EnemySwarmSystem.Instance.nextHitTimes[idx])
            {
                target.TakeDamage(damage);
                EnemySwarmSystem.Instance.nextHitTimes[idx] = Time.time + 0.1f;
            }
        }
        Debug.Log("폭발!");

        ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
    }

    public override void OnHit(Enemy hitEnemy) { }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, impactRadius);
    }
}