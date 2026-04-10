using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TurretBullet : SkillProjectileBase
{
    [SerializeField] public float speed = 5.0f;
    Vector3 direction;
    [SerializeField] private bool IsActived = true;
    public void Fire(Vector3 targetPos, float finalSpeed)
    {
        speed = finalSpeed;
        
        direction = (targetPos - transform.position).normalized;
        IsActived = true;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        IsActived = false;
    }
    void Update()
    {
        if (IsActived)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }
    

    public override void OnHit(Enemy hitEnemy)
    {
        // 데미지는 이미 SkillCollisionManager에서 주었습니다. 
        // 총알은 자폭(Object Pool 반환)과 이펙트 생성만 하면 됩니다.

        
        // 이펙트가 있다면 여기서 생성
        // ObjectPoolingManager.Instance.spawnGameObject(hitEffectPrefab, transform.position, ...);
        
        // 자폭 (오브젝트 풀로 돌아가기)
        ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
    }
}