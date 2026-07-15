using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform handle;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private float handleRange = 80f;

    private RectTransform baseRect;

    private void Awake()
    {
        baseRect = GetComponent<RectTransform>();

        if (inputReader == null)
        {
            inputReader = FindObjectOfType<PlayerInputReader>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateJoystick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateJoystick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = Vector2.zero;

        if (inputReader != null)
        {
            inputReader.ClearMobileMove();
        }
    }

    private void UpdateJoystick(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            baseRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        Vector2 clampedPoint = Vector2.ClampMagnitude(localPoint, handleRange);
        handle.anchoredPosition = clampedPoint;

        Vector2 direction = clampedPoint / handleRange;

        if (inputReader != null)
        {
            inputReader.SetMobileMove(direction);
        }
    }
}