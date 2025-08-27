using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageButtonUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public StageScriptableObject stageInfo;
    private StageSelection stageSelectionManager;
    // Start is called before the first frame update

    private Vector3 normalScale;
    [SerializeField] private float hoverScaleFactor = 1.1f;
    [SerializeField] private float animationDuration = 0.2f;

    [SerializeField] private GameObject HighlightImage;

    public Image CardIcon;
    public TextMeshProUGUI cardText;
    public TMP_Text cardDescription;
    public float glowOutlineWidth = 0.2f;
    private float originalOutlineWidth;
    public Color glowOutlineColor = Color.yellow;

    // Shadow Effect
    private Shadow shadowEffect; // For shadow manipulation
    public Vector2 hoverShadowOffset = new Vector2(5f, -5f);
    private Vector2 originalShadowOffset;

    // Sound effect on hover
    public AudioSource hoverSound;

    // Start is called before the first frame update
    public bool IsSelected;
    public void Initialize(StageSelection manager)
    {
        normalScale = transform.localScale;
        stageSelectionManager = manager;
        CardIcon.sprite = stageInfo.IconSprite;
        cardText.text = stageInfo.StageName;
        cardDescription.text = stageInfo.description;
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
    {if(stageSelectionManager==null) return;
        stageSelectionManager.DeselectOtherButtons();
        stageSelectionManager.SetInfo(stageInfo, this);
        IsSelected = true;
        HighlightImage.SetActive(true);
        // Play hover sound
        if (hoverSound != null)
        {
            hoverSound.Play();
        }

        // Smooth scale up using LeanTween
        LeanTween.scale(gameObject, normalScale * hoverScaleFactor, animationDuration).setEaseOutBack();

    }
    public void DeSelected()
    {
        IsSelected = false;
        // Smooth scale down using LeanTween
        LeanTween.scale(gameObject, normalScale, animationDuration).setEaseInBack();

        HighlightImage.SetActive(false);
        // Reset cursor to default
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    // On click, add a small scale "pop" effect
    public void OnPointerClick(PointerEventData eventData)
    {
        Selected();
    }

}
