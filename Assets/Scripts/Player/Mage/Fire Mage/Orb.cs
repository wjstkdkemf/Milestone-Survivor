using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : SkillProjectileBase
{
    private float chanceDoubleDamage;

    private float hitCooldown = 0.5f; 
    private float trueHitRadius;
    private List<int> enemiesInsideIndices = new List<int>(50);

    public void SetInfo(float _baseDamage, float _hitRadius, float _chanceDoubleDamage , string WeaponID)
    {
        damage = (long)_baseDamage;
        hitRadius = 0f;
        maxHits = -1; 
        
        this.WeaponID = WeaponID;

        chanceDoubleDamage = _chanceDoubleDamage;
        trueHitRadius = _hitRadius;
    }
    protected virtual void Update()
    {
        // 1. 매 프레임 내 주변 적 검색 (초고속 연산)
        EnemySwarmSystem.Instance.GetEnemiesInRadius(transform.position, trueHitRadius, enemiesInsideIndices);

        for (int i = 0; i < enemiesInsideIndices.Count; i++)
        {
            int enemyIdx = enemiesInsideIndices[i];
            
            // 스왑백 및 유령 방어선
            if (enemyIdx >= EnemySwarmSystem.Instance.activeEnemies.Count) continue;
            Enemy target = EnemySwarmSystem.Instance.activeEnemies[enemyIdx];
            if (target == null || target.currentNormalState == Enemy.EnemyState.Dead) continue;

            // 2. 무적 시간 체크 (Job System의 0.1초가 아니라, 오브 전용 0.5초 쿨타임 적용!)
            if (Time.time >= EnemySwarmSystem.Instance.nextHitTimes[enemyIdx])
            {
                float finalDamage = damage;
                if (Random.value < chanceDoubleDamage) finalDamage *= 2f;

                target.TakeDamage(finalDamage);

                // 🚨 0.5초 동안은 이 몬스터가 오브에 또 맞지 않도록 무적 시간을 길게 부여합니다.
                EnemySwarmSystem.Instance.nextHitTimes[enemyIdx] = Time.time + hitCooldown;
            }
        }
    }

    public override void OnHit(Enemy hitEnemy)
    {
    }

}
