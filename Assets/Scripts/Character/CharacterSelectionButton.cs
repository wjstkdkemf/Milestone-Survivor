using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CardUIEffects))]
public class CharacterSelectionButton : MonoBehaviour, IPointerClickHandler
{
    public CharacterScriptableObject characterInfo;
    public Image CardIcon;
    public bool IsSelected;

    [SerializeField] private CardUIEffects cardUIEffects;

    private CharacterSelectionManager characterSelectionManager;
    private Button myButton;

    private void Awake()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        if (cardUIEffects == null)
            cardUIEffects = GetComponent<CardUIEffects>();
        if (myButton == null)
            myButton = GetComponent<Button>();
    }

    public void Initialize(CharacterSelectionManager manager)
    {
        // The manager can initialize cards before their first activation.
        CacheComponents();
        characterSelectionManager = manager;
        IsSelected = false;
        if (cardUIEffects != null)
            cardUIEffects.SetSelected(false, true);

        if (characterInfo == null)
        {
            Debug.LogWarning($"[CharacterSelectionButton] Character info is missing on {name}.");
            return;
        }

        if (CardIcon != null)
        {
            CardIcon.sprite = characterInfo.IconSprite;
            CardIcon.color = characterInfo.purchased ? Color.white : Color.black;
        }
    }

    public void Purchase()
    {
        if (PlayerStats.Instance == null || characterInfo == null)
            return;

        if (PlayerStats.Instance.TrySpendGold(characterInfo.costPerLevel))
            characterInfo.purchased = true;
    }

    public void Selected()
    {
        if (characterSelectionManager == null || characterInfo == null || IsSelected)
            return;

        CacheComponents();
        if (myButton != null && !myButton.IsInteractable())
            return;

        characterSelectionManager.DeselectOtherButtons();
        characterSelectionManager.SetInfo(characterInfo, this);

        IsSelected = true;
        if (cardUIEffects != null)
            cardUIEffects.SetSelected(true);
    }

    public void DeSelected()
    {
        CacheComponents();
        IsSelected = false;
        if (cardUIEffects != null)
            cardUIEffects.SetSelected(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || eventData.button != PointerEventData.InputButton.Left)
            return;
        CacheComponents();
        if (myButton != null && !myButton.isActiveAndEnabled)
            return;

        Selected();
    }
}
