using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트를 처리하기 위해 필수
using TMPro;

public class HoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("커서 오브젝트를 연결하세요")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private RectTransform Cursor;
    [SerializeField] private bool arrowOnRight = true;
    [SerializeField] private float padding = 10f;

    // 마우스가 버튼 영역 안으로 들어올 때 자동으로 실행됨
    public void OnPointerEnter(PointerEventData eventData)
    {
        RefreshArrowPosition();
    }

    // 마우스가 버튼 영역 밖으로 나갈 때 자동으로 실행됨
    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.gameObject.SetActive(false);
    }
    public void RefreshArrowPosition()
    {
        label.ForceMeshUpdate();
        Cursor.gameObject.SetActive(true);

        Bounds bounds = label.textBounds;

        float x = arrowOnRight
            ? bounds.max.x + padding
            : bounds.min.x - padding;

        float y = bounds.center.y;

        Vector2 localPos = Cursor.localPosition;
        localPos.x = label.rectTransform.localPosition.x + x;
        localPos.y = y;
        Cursor.localPosition = localPos;
    }
}