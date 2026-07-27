using UnityEngine;
using UnityEngine.UI;
using InventorySystem;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using TMPro;


public class ItemInfoDisplay : MonoBehaviour
{
    //public TMPro.TextMeshProUGUI infoText; 
    public LocalizeStringEvent[] infoTextEvent;

    //public Sprite itemSprite;
    public Image itemImage;


    public string targetSlotType = "Inventory";
    private LocalizedString localizedDefaultName;
    void Awake()
    {
        UpdateWithDefaultValues();
    }

    void UpdateWithDefaultValues()
    {
        infoTextEvent[0].StringReference.Arguments = new object[] {
            "---", // {0}
        };
        infoTextEvent[1].StringReference.Arguments = new object[] 
        {
            "일반",         // -> {0}
        };
        infoTextEvent[2].StringReference.Arguments = new object[] 
        {
            "설명이 없습니다.",         // -> {0} 
        };


        Refresh();
    }
    void Refresh()
    {
        foreach(var eve in infoTextEvent)
        {
            eve.RefreshString();
        }
    }

    void OnEnable()
    {
        InventoryEventSystem.OnSlotClicked += HandleSlotClick;
    }

    void OnDisable()
    {
        InventoryEventSystem.OnSlotClicked -= HandleSlotClick;
    }

    private async void HandleSlotClick(InventoryItem item, string slotType)
    {
        //if (slotType != targetSlotType)
            //return;
        
        if (item == null || item.GetIsNull() || item.GetAmount() <= 0)
        {
            itemImage.sprite = null;
            UpdateWithDefaultValues();           
            if (ItemEnchanter.Instance != null)
            {
                ItemEnchanter.Instance.SetItem(null);
            }
            return;
        }

        itemImage.sprite = item.GetItemImage();

        LocalizedString localizedItemNameRef = new LocalizedString();
        localizedItemNameRef.TableReference = "Item_Name_Table";      
        localizedItemNameRef.TableEntryReference = item.GetItemType(); // "Item.Name.HealthPotion"

        string translatedItemName = "Loading...";      
        try
        {
            var loadHandle = localizedItemNameRef.GetLocalizedStringAsync();
            translatedItemName = await loadHandle.Task;      
        }
        catch (System.Exception e)
        {
            Debug.LogError($"이름 번역 실패 (Key: {item.GetItemType()}): {e.Message}");
            translatedItemName = item.GetItemType();
        }

        infoTextEvent[0].StringReference.Arguments = new object[] 
        {
            translatedItemName,
        };
        infoTextEvent[1].StringReference.Arguments = new object[] 
        {
            item.GetGrade(),
        };
        infoTextEvent[2].StringReference.Arguments = new object[] 
        {
            item.GetDescription(),
        };

        if (ItemEnchanter.Instance != null)
        {
            ItemEnchanter.Instance.SetItem(item);
        }

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
