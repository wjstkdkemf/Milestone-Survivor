using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class CardUIEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string Description;

    // Scale & Animation settings
    private Vector3 normalScale;
    public float hoverScaleFactor = 1.1f;
    public float animationDuration = 0.2f;

    // Color & Glow effects
    private Graphic cardGraphic;  // For color (could be Image or TextMeshProUGUI)
    public Color hoverColor = Color.yellow; // Color to change to when hovered
    private Color originalColor; // Original color to reset to

    // Text Outline & Glow for TextMeshPro
    public TextMeshProUGUI cardText;
    public float glowOutlineWidth = 0.2f;
    private float originalOutlineWidth;
    public Color glowOutlineColor = Color.yellow;

    // Shadow Effect
    private Shadow shadowEffect; // For shadow manipulation
    public Vector2 hoverShadowOffset = new Vector2(5f, -5f);
    private Vector2 originalShadowOffset;

    // Sound effect on hover
    public AudioSource hoverSound;

    // Cursor
    public Texture2D customCursor;

    void Start()
    {
        normalScale = transform.localScale;

        // Initialize card graphic and colors
        cardGraphic = GetComponent<Graphic>();
        if (cardGraphic != null)
        {
            originalColor = cardGraphic.color;
        }

        // Initialize text outline if using TextMeshPro
        if (cardText != null)
        {
            originalOutlineWidth = cardText.outlineWidth;
        }

        // Initialize shadow effect
        shadowEffect = GetComponent<Shadow>();
        if (shadowEffect != null)
        {
            originalShadowOffset = shadowEffect.effectDistance;
        }
    }

    // On hover, scale up, change color, add glow, and increase shadow
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Play hover sound
        if (hoverSound != null)
        {
            hoverSound.Play();
        }

        // Smooth scale up using LeanTween
        LeanTween.scale(gameObject, normalScale * hoverScaleFactor, animationDuration).setEaseOutBack();

        // Change color
        if (cardGraphic != null)
        {
            cardGraphic.color = hoverColor;
        }

        // Add glow outline for TextMeshPro
        if (cardText != null)
        {
            cardText.outlineWidth = glowOutlineWidth;
            cardText.outlineColor = glowOutlineColor;
        }

        // Increase shadow effect
        if (shadowEffect != null)
        {
            shadowEffect.effectDistance = hoverShadowOffset;
        }

        // Set custom cursor
        if (customCursor != null)
        {
            Cursor.SetCursor(customCursor, Vector2.zero, CursorMode.Auto);
        }
        TooltipManager.instance.SetInfo(Description);
    }

    // On hover exit, scale back to normal, reset color, remove glow, and reset shadow
    public void OnPointerExit(PointerEventData eventData)
    {
        // Smooth scale down using LeanTween
        LeanTween.scale(gameObject, normalScale, animationDuration).setEaseInBack();

        // Reset color
        if (cardGraphic != null)
        {
            cardGraphic.color = originalColor;
        }

        // Remove glow outline for TextMeshPro
        if (cardText != null)
        {
            cardText.outlineWidth = originalOutlineWidth;
        }

        // Reset shadow effect
        if (shadowEffect != null)
        {
            shadowEffect.effectDistance = originalShadowOffset;
        }

        // Reset cursor to default
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    // On click, add a small scale "pop" effect
    public void OnPointerClick(PointerEventData eventData)
    {
        // Create a quick pop animation on click
        LeanTween.scale(gameObject, normalScale * 0.9f, 0.1f).setEaseOutBounce().setOnComplete(() =>
        {
            LeanTween.scale(gameObject, normalScale, 0.1f).setEaseInBounce();
        });
    }
}
