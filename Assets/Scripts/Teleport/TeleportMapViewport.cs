using UnityEngine;
using UnityEngine.EventSystems;

public class TeleportMapViewport : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler
{
    [Header("Targets")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private bool forceCenteredContent = true;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 0.8f;
    [SerializeField] private float maxZoom = 2.5f;
    [SerializeField] private float zoomStep = 0.15f;
    [SerializeField] private float wheelSensitivity = 0.08f;

    [Header("Pan")]
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private float panPadding = 40f;

    private Vector2 lastDragPosition;
    private float currentZoom = 1f;
    private Canvas parentCanvas;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();

        if (viewport == null)
            viewport = transform as RectTransform;

        if (content == null && transform.childCount > 0)
            content = transform.GetChild(0) as RectTransform;

        PrepareContentTransform();
        ApplyZoom(currentZoom);
        ClampContentPosition();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (content == null)
            return;

        float zoomDelta = eventData.scrollDelta.y * wheelSensitivity;
        ZoomTo(currentZoom + zoomDelta, eventData.position, eventData.pressEventCamera);
        eventData.Use();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (content == null)
            return;

        Vector2 screenDelta = eventData.position - lastDragPosition;
        content.anchoredPosition += screenDelta * dragSpeed;
        lastDragPosition = eventData.position;

        ClampContentPosition();
    }

    public void ZoomIn()
    {
        ZoomTo(currentZoom + zoomStep);
    }

    public void ZoomOut()
    {
        ZoomTo(currentZoom - zoomStep);
    }

    public void ResetView()
    {
        currentZoom = 1f;

        if (content != null)
        {
            PrepareContentTransform();
            content.localScale = Vector3.one;
            content.anchoredPosition = Vector2.zero;
        }

        ClampContentPosition();
    }

    public void SetContent(RectTransform newContent)
    {
        content = newContent;
        PrepareContentTransform();
        ResetView();
    }

    private void PrepareContentTransform()
    {
        if (!forceCenteredContent || content == null)
            return;

        content.anchorMin = new Vector2(0.5f, 0.5f);
        content.anchorMax = new Vector2(0.5f, 0.5f);
        content.pivot = new Vector2(0.5f, 0.5f);
    }

    private void ZoomTo(float targetZoom)
    {
        ZoomTo(targetZoom, GetViewportCenterScreenPosition(), null);
    }

    private void ZoomTo(float targetZoom, Vector2 screenPosition, Camera eventCamera)
    {
        if (content == null)
            return;

        float newZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        if (Mathf.Approximately(newZoom, currentZoom))
            return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, screenPosition, eventCamera, out localPoint);

        float previousZoom = currentZoom;
        currentZoom = newZoom;
        ApplyZoom(currentZoom);

        content.anchoredPosition += localPoint * (previousZoom - currentZoom);
        ClampContentPosition();
    }

    private void ApplyZoom(float zoom)
    {
        if (content == null)
            return;

        content.localScale = new Vector3(zoom, zoom, 1f);
    }

    private void ClampContentPosition()
    {
        if (viewport == null || content == null)
            return;

        Vector2 viewportSize = viewport.rect.size;
        Vector2 contentScale = new Vector2(content.localScale.x, content.localScale.y);
        Vector2 contentSize = Vector2.Scale(content.rect.size, contentScale);

        float maxX = Mathf.Max(0f, (contentSize.x - viewportSize.x) * 0.5f) + panPadding;
        float maxY = Mathf.Max(0f, (contentSize.y - viewportSize.y) * 0.5f) + panPadding;

        Vector2 position = content.anchoredPosition;
        position.x = Mathf.Clamp(position.x, -maxX, maxX);
        position.y = Mathf.Clamp(position.y, -maxY, maxY);
        content.anchoredPosition = position;
    }

    private Vector2 GetViewportCenterScreenPosition()
    {
        if (viewport == null)
            return Vector2.zero;

        Vector3 worldCenter = viewport.TransformPoint(viewport.rect.center);
        return RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(), worldCenter);
    }

    private Camera GetCanvasCamera()
    {
        if (parentCanvas == null || parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return parentCanvas.worldCamera;
    }
}
