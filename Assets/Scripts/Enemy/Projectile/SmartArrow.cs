using UnityEngine;

public class SmartArrow : EnemyProjectile
{
    [SerializeField] private float turnSpeed = 200f;

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
        if (target != null && target.gameObject.activeInHierarchy)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        transform.position += transform.right * speed * Time.deltaTime;
    }
}
