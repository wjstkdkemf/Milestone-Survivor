using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialFireballProjectile : MonoBehaviour
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
    private LayerMask enemyLayerMask;
    private HashSet<GameObject> visitedTargets; // 이미 맞춘 적 제외용

    private Vector3 lastSpawnPosition; 
    private DoDamage damageComponent;
    private bool isBouncing = false; // 연쇄 중복 트리거 방지

    // Setup에 체인 관련 변수 3개(chains, cRange, layerMask)와 폭발 크기(boomSize) 추가
    public void Setup(Transform newTarget, float newSpeed, GameObject trail, float tDamage, float tDuration, float tSpawnDist, 
                      GameObject FireBoomPrefab, float FireBoomDamage, float boomSize, int chains, float cRange, LayerMask layerMask)
    {
        target = newTarget;
        speed = newSpeed;
        trailPrefab = trail;
        trailDamage = tDamage;
        trailDuration = tDuration;
        lastFireBoom = FireBoomPrefab;
        spawnDistanceThreshold = tSpawnDist;
        lastFireBoomDamage = FireBoomDamage;
        lastFireBoomSize = boomSize;
        
        remainingChains = chains;
        chainRange = cRange;
        enemyLayerMask = layerMask;
        visitedTargets = new HashSet<GameObject>();

        lastSpawnPosition = transform.position;

        if (target != null)
            RotateTowardsTarget(target.position);
    }

    private void OnEnable()
    {
        damageComponent = GetComponent<DoDamage>();        
        isBouncing = false;
    }

    void Update()
    {
        // 튕기는 처리 중(대기 중)에는 이동 및 장판 생성을 멈춤
        if (isBouncing) return;

        Vector3 moveDirection = transform.right; 

        if (target != null)
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
        GameObject trail = ObjectPoolingManager.Instance.spawnGameObject(trailPrefab, transform.position, Quaternion.identity);
        if (trail.TryGetComponent<DoDamage>(out var doDamage))
        {
            doDamage.damage = trailDamage;           
            doDamage.lifeTime = trailDuration;       
        }
    }
    
    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBouncing) return; // 이미 튕기는 연산 중이면 무시

        if (damageComponent != null && damageComponent.TryCheakEnemy(collision))//&& collision.gameObject == target.gameObject
        {
            isBouncing = true;
            visitedTargets.Add(collision.gameObject); // 맞춘 놈 명단에 추가

            GameObject FireBoom = Instantiate(lastFireBoom, transform.position, Quaternion.identity);
            //FireBoom.transform.localScale = Vector3.one * lastFireBoomSize;

            if (FireBoom.TryGetComponent<DoDamage>(out var boomDamageComponent))
            {
                boomDamageComponent.damage = lastFireBoomDamage;
            }

            StartCoroutine(BounceSequence(transform.position));
        }
    }

    private IEnumerator BounceSequence(Vector3 hitPosition)
    {
        // 핵심: 폭발 데미지가 들어가고 몬스터가 죽을 시간(물리 프레임)을 벌어줌
        // 약간의 딜레이(예: 0.1초)를 주면 폭발로 죽을 놈들은 모두 삭제된 후입니다.
        yield return new WaitForSeconds(0.5f);

        remainingChains--;

        // 남은 체인 횟수가 있다면
        if (remainingChains > 0)
        {
            Transform nextTarget = FindNextTarget(hitPosition);

            if (nextTarget != null)
            {
                // 타겟을 갱신하고 다시 날아감
                target = nextTarget;
                RotateTowardsTarget(target.position);
                lastSpawnPosition = transform.position; // 장판 기준점 리셋
                isBouncing = false; // 다시 Update의 이동 로직이 켜짐
                yield break; 
            }
        }

        // 남은 횟수가 없거나, 타겟을 못 찾았으면 투사체 파괴 (풀링 반환)
        if (damageComponent.IsUsingObjetPooling)
            ObjectPoolingManager.Instance.ReturnObjectToPool(this.gameObject);
        else
            Destroy(gameObject);
    }

    private Transform FindNextTarget(Vector3 center)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, chainRange, enemyLayerMask);
        Transform closestTarget = null;
        float closestDistSqr = Mathf.Infinity;

        foreach (var col in colliders)
        {
            GameObject enemyObj = col.gameObject;
            
            // 이미 방문했거나(방금 맞춘 타겟), 비활성화/죽은 적은 무시
            if (visitedTargets.Contains(enemyObj) || !enemyObj.activeInHierarchy) continue;

            if (col.TryGetComponent<IDamageable>(out _))
            {
                float distSqr = (col.transform.position - center).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closestTarget = col.transform;
                }
            }
        }
        return closestTarget;
    }
}
