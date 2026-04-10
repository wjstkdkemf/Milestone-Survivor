using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

// 이 스크립트는 DoDamage 스크립트와 함께 투사체 프리팹에 붙여주세요.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DoDamage))] 
public class ChainLightningProjectile : SkillProjectileBase
{
    private Enemy currentTarget;
    private float speed;
    private int remainingChains;
    private float chainRange;
    private float damageReduction;
    private HashSet<Enemy> visitedTargets = new HashSet<Enemy>();
    
    private bool isActived = false;

    // 무기 스크립트에서 호출하여 정보 초기화
    public void Fire(Enemy initialTarget, float startDamage, float projSpeed, int chains, float range, float reduction)
    {
        currentTarget = initialTarget;
        damage = startDamage;
        speed = projSpeed;
        remainingChains = chains;
        chainRange = range;
        damageReduction = reduction;
        
        hitRadius = 0.5f; // 투사체 크기
        maxHits = 1;      // 1프레임당 1마리만 때림 (튕기기 위함)
        
        visitedTargets.Clear();
        isActived = true;

        RotateTowardsTarget();
    }

    void Update()
    {
        if (!isActived) return;

        // 🚨 타겟이 내게 오기 전에 다른 공격에 맞아 죽었다면?
        if (currentTarget == null || currentTarget.currentNormalState == Enemy.EnemyState.Dead)
        {
            FindNextTarget(); // 멈추지 않고 주변의 다른 놈으로 즉시 궤도 수정!
            if (!isActived) return; 
        }

        // 타겟을 향해 이동 (직진이 아니라 유도탄 방식)
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        RotateTowardsTarget();
    }
    private void RotateTowardsTarget()
    {
        if (currentTarget == null) return;
        Vector3 direction = currentTarget.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        isActived = false;
        visitedTargets.Clear();
    }
    public override void OnHit(Enemy hitEnemy)
    {
        visitedTargets.Add(hitEnemy);

        remainingChains--;
        damage *= (1f - damageReduction);

        if (remainingChains > 0)
        {
            FindNextTarget();
        }
        else
        {
            SelfDestruct();
        }
    }

    private void FindNextTarget()
    {
        Enemy nextTarget = EnemySwarmSystem.Instance.GetClosestEnemy(transform.position, chainRange, visitedTargets);

        if (nextTarget != null)
        {
            currentTarget = nextTarget;
            RotateTowardsTarget();
        }
        else
        {
            SelfDestruct(); 
        }
    }

    private void SelfDestruct()
    {
        isActived = false;
        ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
    }
}