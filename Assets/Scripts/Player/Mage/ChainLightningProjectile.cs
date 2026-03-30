using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

// 이 스크립트는 DoDamage 스크립트와 함께 투사체 프리팹에 붙여주세요.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DoDamage))] 
public class ChainLightningProjectile : MonoBehaviour
{
    private Transform target;
    private float speed;
    private int remainingChains;
    private float chainRange;
    private float damageReduction;
    private LayerMask enemyLayerMask;
    private HashSet<GameObject> visitedTargets; // 이미 튕긴 적들 기록

    private bool hasChained = false; // 중복 체인 방지용
    private DoDamage damageComponent;
    private GameObject originalPrefab;

    // 무기 스크립트에서 호출하여 정보 초기화
    public void Setup(Transform newTarget, float newSpeed, int chains, float range, float reduction, LayerMask layer, HashSet<GameObject> visited , GameObject originalPrefabRef)
    {
        target = newTarget;
        speed = newSpeed;
        remainingChains = chains;
        chainRange = range;
        damageReduction = reduction;
        enemyLayerMask = layer;
        visitedTargets = visited;

        originalPrefab = originalPrefabRef;

        hasChained = false;

        damageComponent = GetComponent<DoDamage>();
        if (damageComponent != null)
        {
            //damageComponent.enabled = false; 
            
            // 만약 DoDamage의 SelfDestroy 코루틴이 필요하다면 수동으로 실행해줘야 할 수 있으나,
            // 보통 투사체는 타겟에 닿으면 즉시 사라지므로 LifeTime만 체크하면 됩니다.
            // 여기서는 안전하게 LifeTime 뒤 파괴 로직을 별도 코루틴으로 돌리거나, 
            // DoDamage의 Start()는 실행되게 놔두고 Update/OnTrigger만 막는 방식(enabled=false)이 유효합니다.
        }

        // 타겟을 향해 회전 (시각적 효과)
        if (target != null)
        {
            RotateTowardsTarget();
        }
    }

    void Update()
    {
        if (target != null)
        {
            // 타겟을 향해 이동
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            RotateTowardsTarget();
        }
        else
        {
            // 타겟이 죽거나 사라졌다면? -> 그냥 직진하다가 DoDamage의 LifeTime(혹은 별도 처리)에 의해 소멸
            transform.position += transform.right * speed * Time.deltaTime;
        }
    }
    void OnEnable()
    {
        hasChained = false;

        if(damageComponent == null) damageComponent = GetComponent<DoDamage>();
        //if(damageComponent != null) damageComponent.enabled = false;
    }
    private void RotateTowardsTarget()
    {
        if (target == null) return;
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasChained || (enemyLayerMask.value & (1 << collision.gameObject.layer)) == 0) 
            return;

        if (visitedTargets.Contains(collision.gameObject)) 
            return;

        HitTarget(collision);
    }

    private void HitTarget(Collider2D collision)
    {
        hasChained = true;
        visitedTargets.Add(collision.gameObject);

        // 2. 다음 체인 실행
        if (remainingChains > 0)
        {
            ChainToNextTarget(collision.transform.position);
        }
        if (damageComponent != null)
        {
            damageComponent.TryApplyDamage(collision);
        }
    }

    private void ChainToNextTarget(Vector3 currentHitPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(currentHitPosition, chainRange, enemyLayerMask);
        Transform closestNextTarget = null;
        float closestDistSqr = Mathf.Infinity;

        foreach (var col in colliders)
        {
            GameObject enemyObj = col.gameObject;
            if (visitedTargets.Contains(enemyObj)) continue; // 이미 맞은 놈 제외

            if (col.TryGetComponent<IDamageable>(out _))
            {
                float distSqr = (col.transform.position - currentHitPosition).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closestNextTarget = col.transform;
                }
            }
        }

        if (closestNextTarget != null)
        {
            SpawnNextProjectile(closestNextTarget);
        }
    }

    private void SpawnNextProjectile(Transform nextTarget)
    {
        GameObject prefabToSpawn = originalPrefab != null ? originalPrefab : gameObject;


        GameObject nextProjectile = ObjectPoolingManager.Instance.spawnGameObject(prefabToSpawn, transform.position, Quaternion.identity);

        // 다음 투사체의 DoDamage 수치 조절
        if (nextProjectile.TryGetComponent<DoDamage>(out var nextDamageComponent))
        {
            // 현재 내 데미지 * 감소율
            nextDamageComponent.damage = damageComponent.damage * damageReduction;
        }

        // 체인 정보 설정
        if (nextProjectile.TryGetComponent<ChainLightningProjectile>(out var nextChainComponent))
        {
            nextChainComponent.Setup(nextTarget, speed, remainingChains - 1, chainRange, damageReduction, enemyLayerMask, visitedTargets, originalPrefab);
        }
    }
}