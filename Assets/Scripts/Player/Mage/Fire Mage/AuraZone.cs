using UnityEngine;

public class AuraZone : ZoneDamageArea
{
    private bool applySlow;
    private float slowPercentage;

    // 무기 관리자에서 호출하여 오라 정보 주입
    public void SetAuraInfo(float rate, float damage, bool doSlow, float slowPct)
    {
        this.tickRate = rate;
        this.applySlow = doSlow;
        this.slowPercentage = slowPct;

        if (damageComponent != null)
        {
            damageComponent.damage = damage;
            // 오라는 무한 지속이므로 파괴 옵션을 끕니다.
            damageComponent.selfDestroy = false;
            damageComponent.destroyAfterHit = false; 
        }
    }

    // 영역에 들어왔을 때 (슬로우 부여)
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision); // 부모 로직(명단 추가) 먼저 실행

        if (applySlow)
        {
            // 몬스터의 이동 스크립트를 가져와서 슬로우를 겁니다.
            // (주의: 님의 프로젝트에 있는 실제 몬스터 스크립트 이름으로 변경해야 합니다!)
            
            if (collision.TryGetComponent<Enemy>(out var enemyMove))
            {
                enemyMove.ApplySlow(slowPercentage);
            }
        }
    }

    // 영역에서 나갔을 때 (슬로우 해제)
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision); // 부모 로직(명단 제거) 먼저 실행

        if (applySlow)
        {
            // 영역에서 벗어나면 원래 속도로 복구합니다.
            
            if (collision.TryGetComponent<Enemy>(out var enemyMove))
            {
                enemyMove.ResetStatusEffects();
            }
        }
    }
}