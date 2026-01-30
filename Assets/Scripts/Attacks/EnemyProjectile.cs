using UnityEngine;

// 모든 적 투사체는 이 클래스를 상속받아야 합니다.
public abstract class EnemyProjectile : MonoBehaviour
{
    protected float damage;
    protected float speed;
    protected Transform target;

    public abstract void Setup(Transform target, float speed, float damage);

}