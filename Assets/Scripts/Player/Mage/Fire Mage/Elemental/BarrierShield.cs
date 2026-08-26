using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BarrierShield : MonoBehaviour
{
    private BarrierWeapon parentWeapon;
    private LayerMask enemyLayerMask;
    private LayerMask projectileLayerMask;


    public void Setup(BarrierWeapon weapon, LayerMask layers , LayerMask projectLayerMask)
    {
        parentWeapon = weapon;
        enemyLayerMask = layers;
        projectileLayerMask = projectLayerMask;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 적 레이어인지 확인
        if (((1 << collision.gameObject.layer) & enemyLayerMask) != 0)
        {
            // 적이나 적 투사체와 닿았다면 배리어 파괴 트리거
            TriggerBreak();

            // 중요: 투사체의 경우 여기서 즉시 파괴하여 데미지를 무효화
            // (적 본체라면 넉백으로 밀려나서 데미지 판정이 안 들어가게 됨)
            if (((1 << collision.gameObject.layer) & projectileLayerMask) != 0)
            {
                 // 상대방의 DoDamage 컴포넌트를 비활성화하거나 오브젝트 파괴
                 // (여기서는 투사체인 경우만 파괴하도록 체크 필요, 혹은 단순히 밀어내기)
                 DevLog.Log("투사체 충돌 확인용");
                 Destroy(collision.gameObject);
            }
        }
    }

    private void TriggerBreak()
    {
        if (parentWeapon != null)
        {
            parentWeapon.BreakBarrier();
        }
    }
}
