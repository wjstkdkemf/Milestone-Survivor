using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LoadSceneSelectionButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Slot Info")]
    public int slotId;

    [Header("UI References")]
    public Image characterImage; // Note: Character sprite isn't in the save data yet
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

        // Try to get the save data for this slot
        if (SaveLoadManager.Instance != null)
        {
            GameSaveData saveData = SaveLoadManager.Instance.GetSaveSlotData(slotId);

            if (saveData != null && saveData.playerStatsData != null)
            {
                // We have data, update the UI
                levelText.text = "Gold: " + saveData.playerStatsData.goldAmount;
                // You could also show player level, etc.
                // levelText.text += "\nLevel: " + saveData.playerStatsData.level;
                if(characterImage) characterImage.enabled = true; // Or set sprite based on CharacterID
            }
            else
            {
                // No save data for this slot
                levelText.text = "Empty Slot";
                if(characterImage) characterImage.enabled = false;
            }
        }
        else
        {
            levelText.text = "Error: SLM not found";
        }
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