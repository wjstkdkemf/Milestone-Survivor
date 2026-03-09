using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;


public class CharacterSelectionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public CharacterScriptableObject characterInfo;
    private CharacterSelectionManager characterSelectionManager;
    private Vector3 normalScale;
    public float hoverScaleFactor = 1.1f;
    public float animationDuration = 0.2f;

    // Color & Glow effects
    private Graphic cardGraphic;  // For color (could be Image or TextMeshProUGUI)
    public Color hoverColor = Color.yellow; // Color to change to when hovered
    private Color originalColor; // Original color to reset to

    // Text Outline & Glow for TextMeshPro
    public Image CardIcon;
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
    // Start is called before the first frame update
    public bool IsSelected;
    public void Initialize(CharacterSelectionManager manager)
    {
        characterSelectionManager = manager;
        if (characterInfo.purchased == false)
            CardIcon.color = Color.black;
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
        CardIcon.sprite = characterInfo.IconSprite;
        // Initialize shadow effect
        shadowEffect = GetComponent<Shadow>();
        if (shadowEffect != null)
        {
            originalShadowOffset = shadowEffect.effectDistance;
        }
    }

    public void Purchase()
    {
        if (characterInfo.costPerLevel >= PlayerStats.Instance.GoldAmount)
        {
            PlayerStats.Instance.GoldAmount -= characterInfo.costPerLevel;
            characterInfo.purchased = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Play hover sound
        if (hoverSound != null)
        {
            hoverSound.Play();
        }

        // Smooth scale up using LeanTween
        LeanTween.scale(gameObject, normalScale * hoverScaleFactor, animationDuration).setEaseOutBack();
    }

    // On hover exit, scale back to normal, reset color, remove glow, and reset shadow
    public void OnPointerExit(PointerEventData eventData)
    {
        // Smooth scale down using LeanTween
        if (IsSelected == false)
            LeanTween.scale(gameObject, normalScale, animationDuration).setEaseInBack();


    }

    public void Selected()
    {
        characterSelectionManager.DeselectOtherButtons();
        characterSelectionManager.SetInfo(characterInfo, this);
        IsSelected = true;
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
    }
    public void DeSelected()
    {
        IsSelected = false;
        // Smooth scale down using LeanTween
        LeanTween.scale(gameObject, normalScale, animationDuration).setEaseInBack();

        // Reset color
        if (cardGraphic != null)
        {
            cardGraphic.color = originalColor;
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
        Selected();
    }
}
