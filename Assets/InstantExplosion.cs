using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class InstantExplosion : MonoBehaviour
{
private DoDamage damageComponent;
    
    [Header("Explosion Settings")]
    public float explosionRadius = 3f;
    public LayerMask targetLayerMask; // DoDamage의 레이어와 동일하게 맞춰주면 좋습니다.

    private bool hasExploded = false; // 중복 폭발 방지용

    private void OnEnable()
    {
        damageComponent = GetComponent<DoDamage>();
        targetLayerMask = damageComponent.enemyLayer;

        Invoke(nameof(Explode), 0.3f);
    }

    // 메테오가 땅에 닿는 순간, 혹은 폭발 애니메이션의 특정 프레임에 이 함수를 호출합니다.
    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 1. 지정된 반경(explosionRadius) 내의 모든 타겟을 단 한 프레임 만에 즉시 긁어옵니다.
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetLayerMask);

        // 2. 수집된 명단 전체에 데미지 통보
        foreach (Collider2D hit in hitColliders)
        {
            Debug.Log("체크용");
            if (damageComponent != null)
            {
                // [새로운 룰 적용] 폭발에 휘말린 대상에게 딜을 넣습니다.
                damageComponent.TryApplyDamage(hit);
            }
        }

        // 폭발 처리가 끝났으므로 오브젝트 파괴 (이펙트 잔류 시간을 위해 딜레이 파괴)
        // 만약 풀링을 쓴다면 풀 반환 코루틴을 실행하면 됩니다.
        //Destroy(gameObject, 1.5f); 
    }
    public void DestroyObject() 
    {
        // 애니메이션이 끝나는 시점에 오브젝트를 제거
        Destroy(gameObject);
    }

    // 유니티 에디터에서 폭발 범위를 시각적으로 확인하기 위한 기능입니다.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
