using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트를 처리하기 위해 필수

public class HoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("커서 오브젝트를 연결하세요")]
    public GameObject leftCursor;
    public GameObject rightCursor;

    // 마우스가 버튼 영역 안으로 들어올 때 자동으로 실행됨
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (leftCursor != null) leftCursor.SetActive(true);
        if (rightCursor != null) rightCursor.SetActive(true);
    }

    // 마우스가 버튼 영역 밖으로 나갈 때 자동으로 실행됨
    public void OnPointerExit(PointerEventData eventData)
    {
        if (leftCursor != null) leftCursor.SetActive(false);
        if (rightCursor != null) rightCursor.SetActive(false);
    }
}