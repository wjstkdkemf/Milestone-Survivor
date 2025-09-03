using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LoadSceneSelectionButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Slot Info")]
    public int slotId;

    [Header("UI References")]
    public Image characterImage;
    public TextMeshProUGUI levelText;

    [Header("Selection Visuals")]
    public Color selectedColor = Color.yellow;
    private Color originalColor;
    private Image buttonImage;

    private bool isSelected = false;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }

        // --- YOU WILL UPDATE THE UI WITH SAVED DATA HERE ---
        // This is where you would load data for this specific slot
        // and update the characterImage and levelText.
        // For example:
        // SavedData data = SaveLoadManager.Instance.GetSlotData(slotId);
        // if (data != null) {
        //     characterImage.sprite = data.characterSprite;
        //     levelText.text = "Level: " + data.level;
        // } else {
        //     // Handle empty slot appearance
        //     levelText.text = "Empty";
        //     characterImage.enabled = false;
        // }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (LoadScreenManager.Instance != null)
        {
            LoadScreenManager.Instance.SelectSlot(this);
        }
    }

    public void Select()
    {
        isSelected = true;
        if (buttonImage != null)
        {
            buttonImage.color = selectedColor;
        }
    }

    public void Deselect()
    {
        isSelected = false;
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }
}
