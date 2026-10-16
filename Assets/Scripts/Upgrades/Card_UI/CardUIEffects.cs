using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUIEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 normalScale;
    public float hoverScaleFactor = 1.1f;
    public float animationDuration = 0.2f;
    private int scaleTweenId = -1;
    private Vector3 scaleTweenTarget;
    private Graphic cardGraphic;
    public AudioSource hoverSound;
    public Texture2D customCursor;
    public Color hoverColor = Color.yellow;
    private Color originalColor;
    private bool visualStateInitialized;
    public bool IsSelected = false;
    private bool isHovered;
    private Selectable selectable;

    private void OnEnable()
    {
        RefreshVisual(true);
    }

    private void OnDisable()
    {
        isHovered = false;
        CancelScaleTween();
        if (!visualStateInitialized) return;

        transform.localScale = normalScale;
        if (cardGraphic != null)
            cardGraphic.color = originalColor;
        if (customCursor != null)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;
        EnsureVisualStateInitialized();
        if (selectable != null && (!selectable.isActiveAndEnabled || !selectable.IsInteractable()))
            return;

        if (!isHovered && !IsSelected && hoverSound != null)
            hoverSound.Play();
        isHovered = true;
        RefreshVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;
        isHovered = false;
        RefreshVisual();
    }

    public void EnsureVisualStateInitialized()
    {
        if (visualStateInitialized) return;

        normalScale = transform.localScale;
        cardGraphic = GetComponent<Graphic>();
        selectable = GetComponent<Selectable>();

        if (cardGraphic != null)
            originalColor = cardGraphic.color;

        visualStateInitialized = true;
    }
    public void SetSelected(bool selected, bool immediate = false)
    {
        EnsureVisualStateInitialized();
        bool playSound = selected && !IsSelected && !isHovered;
        IsSelected = selected;

        if (playSound && isActiveAndEnabled && !immediate && hoverSound != null)
            hoverSound.Play();

        RefreshVisual(immediate);
    }

    private void RefreshVisual(bool immediate = false)
    {
        EnsureVisualStateInitialized();
        bool highlighted = isActiveAndEnabled && (IsSelected || isHovered)
            && (selectable == null || (selectable.isActiveAndEnabled && selectable.IsInteractable()));
        Vector3 targetScale = highlighted
            ? normalScale * hoverScaleFactor
            : normalScale;

        ChangeScale(targetScale, immediate);

        if (cardGraphic != null)
            cardGraphic.color = highlighted ? hoverColor : originalColor;

        if (customCursor != null)
            Cursor.SetCursor(
                highlighted ? customCursor : null,
                Vector2.zero,
                CursorMode.Auto);
    }
    public void ChangeScale(Vector3 target, bool immediate = false)
    {
        // Hover followed by click must not restart a tween already moving to the same size.
        if (!immediate && isActiveAndEnabled && animationDuration > 0f
            && scaleTweenId >= 0 && LeanTween.isTweening(scaleTweenId)
            && scaleTweenTarget == target)
            return;

        CancelScaleTween();

        if(immediate || !isActiveAndEnabled || animationDuration <= 0f)
        {
            transform.localScale = target;
            return;
        }

        if(transform.localScale == target)
        {
            transform.localScale = target;
            return;
        }

        scaleTweenTarget = target;
        scaleTweenId = LeanTween.scale(gameObject, target, animationDuration)
                                .setIgnoreTimeScale(true)
                                .setEase(LeanTweenType.easeOutQuad)
                                .setOnComplete(() =>
                                {
                                    transform.localScale = target;
                                    scaleTweenId = -1;
                                })
                                .id;
    }
    private void CancelScaleTween()
    {
        if (scaleTweenId < 0) return;

        LeanTween.cancel(scaleTweenId);
        scaleTweenId = -1;
    }
}
