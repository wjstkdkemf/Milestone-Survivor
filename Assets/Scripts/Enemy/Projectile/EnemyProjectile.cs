using UnityEngine;

// 모든 적 투사체는 이 클래스를 상속받아야 합니다.
public abstract class EnemyProjectile : MonoBehaviour
{
    protected float damage;
    protected float speed;
    protected Transform target;
    protected bool hasHit;

    protected DoDamage damageComponent;

    public abstract void Setup(Transform target, float speed, float damage);
    protected virtual void Awake()
    {
        damageComponent = GetComponent<DoDamage>();
    }
    private void OnEnable()
    {
        hasHit = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (damageComponent != null && damageComponent.TryApplyDamage(collision))
        {
            hasHit = true; 
            //HandleSelfDestruction();
        }
    }
}