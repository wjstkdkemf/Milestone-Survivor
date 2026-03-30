using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DoDamage))]
public class Arrow : EnemyProjectile
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void Setup(Transform target, float speed, float damage)
    {
        this.target = target;
        this.speed = speed;
        this.damage = damage;

        if (damageComponent != null)
        {
            damageComponent.damage = this.damage;
            damageComponent.lifeTime = lifeTime;
        }

        // 초기화 시점에만 타겟 방향으로 회전하고 끝 (이후로는 직진)
        if (this.target != null)
        {
            Vector2 direction = (this.target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnEnable()
    {
        CancelInvoke(nameof(SelfDestroy));
        Invoke(nameof(SelfDestroy), lifeTime);
    }

    private void FixedUpdate()
    {
        // 유도 로직 없이 바라보는 방향(transform.right)으로만 계속 전진
        rb.velocity = transform.right * speed;
    }

    private void SelfDestroy()
    {
        if (gameObject.activeInHierarchy)
        {
            if (ObjectPoolingManager.Instance != null)
                ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
            else
                Destroy(gameObject);
        }
    }
}