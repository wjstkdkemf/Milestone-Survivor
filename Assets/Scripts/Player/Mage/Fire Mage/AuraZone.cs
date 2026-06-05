using UnityEngine;
using System.Collections.Generic;


public class AuraZone : SkillProjectileBase
{
    private float tickRate;
    private bool applySlow;
    private float slowPercentage;
    private float damageTimer;
    private float slowRefreshTimer;

    private float lifeTimer = 0f;
    private bool hasLifeTime = false;

    private List<int> enemiesInsideIndices = new List<int>(100);

    public void SetupAura(float rate, long dmg, float rad, bool doSlow, float slowPct , string WeaponID, float duration = 0f)
    {
        damage = dmg;
        hitRadius = rad;
        maxHits = -1;

        tickRate = rate;
        applySlow = doSlow;
        slowPercentage = slowPct;

        damageTimer = 0f;
        slowRefreshTimer = 0f;

        this.WeaponID = WeaponID;

        if (duration > 0f)
        {
            lifeTimer = duration;
            hasLifeTime = true;
        }
        else
        {
            hasLifeTime = false; //무한 지속
        }
    }

    protected virtual void Update()
    {
        if (hasLifeTime)
        {
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
                return;
            }
        }
        bool timeToDamage = (damageTimer -= Time.deltaTime) <= 0f;
        bool timeToSlow = applySlow && (slowRefreshTimer -= Time.deltaTime) <= 0f;

        if (!timeToDamage && !timeToSlow) return;

        EnemySwarmSystem.Instance.GetEnemiesInRadius(transform.position, hitRadius, enemiesInsideIndices);

        for (int i = 0; i < enemiesInsideIndices.Count; i++)
        {
            int enemyIdx = enemiesInsideIndices[i];
            if (enemyIdx >= EnemySwarmSystem.Instance.activeEnemies.Count) continue;

            Enemy target = EnemySwarmSystem.Instance.activeEnemies[enemyIdx];

            if (target == null || target.currentNormalState == Enemy.EnemyState.Dead) continue;

            if (timeToSlow)
            {
                target.ApplySlow(slowPercentage, 0.2f); 
            }

            if (timeToDamage)
            {
                if (Time.time >= EnemySwarmSystem.Instance.nextHitTimes[enemyIdx])
                {
                    target.TakeDamage(damage);
                    RunStatisticsManager.Instance.RecordWeaponDamage(WeaponID, damage);
                    EnemySwarmSystem.Instance.nextHitTimes[enemyIdx] = Time.time + 0.1f; 
                }
            }
        }

        if (timeToDamage) damageTimer = tickRate;
        if (timeToSlow) slowRefreshTimer = 0.1f; 
    }

    public override void OnHit(Enemy hitEnemy)
    {
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, hitRadius);
    }
}