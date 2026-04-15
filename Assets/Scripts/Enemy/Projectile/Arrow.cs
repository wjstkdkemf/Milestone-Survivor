using UnityEngine;

public class Arrow : EnemyProjectile
{
    public override void Setup(Transform target, float speed, float damage, float lifeTime = 5f)
    {
        base.Setup(target, speed, damage, lifeTime);

        if (this.target != null)
        {
            Vector2 direction = (this.target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    protected override void Move()
    {
        // 순수 수학으로 전진! (물리 연산 0%)
        transform.position += transform.right * speed * Time.deltaTime;
    }
}