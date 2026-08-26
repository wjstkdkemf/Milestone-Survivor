using UnityEngine;
using UnityEngine.UI;
using InventorySystem;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.Localization.Settings;


public class ItemInfoDisplay : MonoBehaviour
{
    //public TMPro.TextMeshProUGUI infoText; 
    public LocalizeStringEvent[] infoTextEvent;

    //public Sprite itemSprite;
    public Image itemImage;


    public string targetSlotType = "Inventory";
    private LocalizedString localizedDefaultName;
    private InventoryItem currentItem;
    void Awake()
    {
        UpdateWithDefaultValues();
    }

    void UpdateWithDefaultValues()
    {
        if(infoTextEvent == null || infoTextEvent.Length < 3)
            return;
        
        infoTextEvent[0].StringReference.Arguments = new object[] {
            "---", // {0}
        };
        infoTextEvent[1].StringReference.Arguments = new object[] 
        {
            "---",         // -> {0}
        };
        infoTextEvent[2].StringReference.Arguments = new object[] 
        {
            "---",         // -> {0} 
        };


        Refresh();
    }
    void Refresh()
    {
        foreach(var eve in infoTextEvent)
        {
            if(eve != null)
                eve.RefreshString();
        }
    }

    void OnEnable()
    {
        InventoryEventSystem.OnSlotClicked += HandleSlotClick;
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    void OnDisable()
    {
        InventoryEventSystem.OnSlotClicked -= HandleSlotClick;
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    private void HandleLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        if (currentItem != null && !currentItem.GetIsNull() && currentItem.GetAmount() > 0)
            UpdateItemInfo(currentItem);
    }

    private void HandleSlotClick(InventoryItem item, string slotType)
    {
        //if (slotType != targetSlotType)
            //return;
        
        if (item == null || item.GetIsNull() || item.GetAmount() <= 0)
        {
            currentItem = null;
            itemImage.sprite = null;
            UpdateWithDefaultValues();           
            if (ItemEnchanter.Instance != null)
            {
                ItemEnchanter.Instance.SetItem(null);
            }
            return;
        }

        currentItem = item;
        UpdateItemInfo(item);

        if (ItemEnchanter.Instance != null)
        {
            ItemEnchanter.Instance.SetItem(item);
        }
    }

    private void UpdateItemInfo(InventoryItem item)
    {
        itemImage.sprite = item.GetItemImage();
        if(infoTextEvent == null || infoTextEvent.Length < 3)
            return;

        infoTextEvent[0].StringReference.Arguments = new object[] 
        {
            item.GetLocalizedName(),
        };
        infoTextEvent[1].StringReference.Arguments = new object[] 
        {
            item.GetGrade(),
        };
        infoTextEvent[2].StringReference.Arguments = new object[] 
        {
            item.GetLocalizedDescription(),
        };

        Refresh();
    }

    /*private string BuildDescriptionText(InventoryItem item)
    {
        string description = item.GetDescription();
        List<StatModifier> modifiers = item.GetStatModifiers();

        if (modifiers == null || modifiers.Count == 0)
        {
            return description;
        }

        StringBuilder builder = new StringBuilder();
        if (!string.IsNullOrEmpty(description))
        {
            builder.AppendLine(description);
        }

        foreach (StatModifier modifier in modifiers)
        {
            if (string.IsNullOrEmpty(modifier.statName)) continue;
            builder.AppendLine($"{modifier.statName}: {modifier.value:+0.##;-0.##;0}");
        }

        return builder.ToString().TrimEnd();
    }*/
}
