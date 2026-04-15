using UnityEngine;

// 모든 적 투사체는 이 클래스를 상속받아야 합니다.
public abstract class EnemyProjectile : MonoBehaviour
{
    public float damage;
    protected float speed;
    protected Transform target;
    protected bool isActived;
    
    private float lifeTimer;
    public virtual void Setup(Transform target, float speed, float damage, float lifeTime = 5f)
    {
        this.target = target;
        this.speed = speed;
        this.damage = damage;
        this.lifeTimer = lifeTime;
        this.isActived = true;
    }
    private void OnEnable()
    {
        isActived = true;

        if (EnemyProjectileManager.Instance != null)
        {
            EnemyProjectileManager.Instance.RegisterProjectile(this);
        }
    }
    protected virtual void Update()
    {
        if (!isActived) return;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            SelfDestroy();
            return;
        }

        Move(); 
    }
    protected abstract void Move();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActived) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            SelfDestroy();
        }
    }

    public void SelfDestroy()
    {
        isActived = false;
        if (ObjectPoolingManager.Instance != null && gameObject.activeInHierarchy)
        {
            ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
        }
    }
}