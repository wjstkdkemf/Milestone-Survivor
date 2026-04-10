using UnityEngine;
using System.Collections.Generic;


public class AuraZone : SkillProjectileBase
{
private float tickRate;
    private bool applySlow;
    private float slowPercentage;
    
    private float damageTimer;
    private float slowRefreshTimer;

    // 가비지 생성을 막기 위한 인덱스 바구니
    private List<int> enemiesInsideIndices = new List<int>(100);

    // 💡 투사체의 Fire() 대신 오라에 맞는 셋업 함수를 만듭니다.
    public void SetupAura(float rate, float dmg, float rad, bool doSlow, float slowPct)
    {
        this.damage = dmg;
        this.hitRadius = rad;
        this.maxHits = -1;

        this.tickRate = rate;
        this.applySlow = doSlow;
        this.slowPercentage = slowPct;

        this.damageTimer = 0f;
        this.slowRefreshTimer = 0f;
    }

    protected virtual void Update()
    {
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
                    EnemySwarmSystem.Instance.nextHitTimes[enemyIdx] = Time.time + 0.1f; 
                }
            }
        }

        // 타이머 리셋
        if (timeToDamage) damageTimer = tickRate;
        if (timeToSlow) slowRefreshTimer = 0.1f; 
    }

    // 오라는 Job System의 OnHit을 안 쓸 수도 있으므로 빈 함수로 오버라이드
    public override void OnHit(Enemy hitEnemy) { }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, hitRadius);
    }
}