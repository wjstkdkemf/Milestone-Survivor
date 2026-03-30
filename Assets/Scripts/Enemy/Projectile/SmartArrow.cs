using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DoDamage))]
public class SmartArrow : EnemyProjectile // [수정] MonoBehaviour 대신 EnemyProjectile 상속
{
    [Header("Homing Settings")]
    [SerializeField] private float turnSpeed = 200f;
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody2D rb;
    

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    // [수정] 추상 메서드 Setup 구현 (override 필수)
    public override void Setup(Transform target, float speed, float damage)
    {
        // 부모 클래스의 변수에 저장 (선택 사항, 자식에서 따로 써도 됨)
        this.target = target;
        this.speed = speed;
        this.damage = damage;

        // DoDamage 컴포넌트에 데미지 주입
        if (damageComponent != null)
        {
            damageComponent.damage = this.damage;
            damageComponent.lifeTime = lifeTime;
        }

        // 초기화 시 타겟 방향 바라보기
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
        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - rb.position;
            direction.Normalize();
            float rotateAmount = Vector3.Cross(direction, transform.right).z;
            rb.angularVelocity = -rotateAmount * turnSpeed;
        }
        else
        {
            rb.angularVelocity = 0f;
        }

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
