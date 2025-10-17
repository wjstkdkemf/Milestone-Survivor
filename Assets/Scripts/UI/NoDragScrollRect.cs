using UnityEngine.EventSystems;
using UnityEngine.UI;

// ScrollRect를 상속받아 새로운 컴포넌트를 만듭니다.
public class NoDragScrollRect : ScrollRect
{
    // OnBeginDrag 이벤트가 발생했을 때 아무것도 하지 않도록 오버라이드합니다.
    public override void OnBeginDrag(PointerEventData eventData)
    {
        // base.OnBeginDrag(eventData); // 기본 기능을 주석 처리하거나 비워 둡니다.
    }

    // OnDrag 이벤트가 발생했을 때 아무것도 하지 않도록 오버라이드합니다.
    public override void OnDrag(PointerEventData eventData)
    {
        // base.OnDrag(eventData); // 기본 기능을 주석 처리하거나 비워 둡니다.
    }

    // OnEndDrag 이벤트가 발생했을 때 아무것도 하지 않도록 오버라이드합니다.
    public override void OnEndDrag(PointerEventData eventData)
    {
        // base.OnEndDrag(eventData); // 기본 기능을 주석 처리하거나 비워 둡니다.
    }
}
