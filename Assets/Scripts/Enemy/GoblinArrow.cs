
using UnityEngine;

public class GoblinArrow : MonoBehaviour
{
    public float damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // "Player" 태그를 가진 오브젝트와 충돌했는지 확인
        if (collision.gameObject.CompareTag("Player"))
        {
            // IDamageable 인터페이스를 통해 플레이어에게 데미지를 줌
            IDamageable player = collision.gameObject.GetComponent<IDamageable>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }

        // 플레이어가 아닌 다른 콜라이더(예: 벽)에 닿아도 화살이 사라지도록 설정
        if (!collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
