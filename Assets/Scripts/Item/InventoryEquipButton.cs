using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class InventoryEquipButton : MonoBehaviour
{
    [SerializeField] 
    private Button actionButton;
    [SerializeField]
    private LocalizeStringEvent actionTextEvent;

    [SerializeField]
    private LocalizedString equipText;

    [SerializeField]
    private LocalizedString unequipText;

    private InventoryItem selectedItem;

    private void OnEnable()
    {
        selectedItem = null;
        InventoryEventSystem.OnSlotClicked += HandleSlotClicked;
        RefreshButton();
    }

    private void OnDisable()
    {
        InventoryEventSystem.OnSlotClicked -= HandleSlotClicked;
    }

    private void HandleSlotClicked(InventoryItem item, string slotType)
    {
        selectedItem =
            item != null && !item.GetIsNull() ? item : null;

        RefreshButton();
    }

    public void ExecuteSelectedItemAction()
    {
        if (selectedItem == null ||
            InventoryController.instance == null)
        {
            return;
        }

        bool isHotbar =
            selectedItem.GetInventory() ==
            InventoryController.HotBarInventoryName;

        bool IsEquip = selectedItem.GetEquit();

        if (isHotbar)
        {
            InventoryController.instance
                .UnequipItemFromHotbar(selectedItem);

            selectedItem = null;
        }
        else if (IsEquip)
        {
            InventoryController.instance.UnequipItemFromInventory(selectedItem);
        }
        else if (selectedItem.GetEquipmentType() != EquipmentType.None)
        {
            InventoryController.instance.EquipItem(
                selectedItem,
                actionButton.transform.position
            );
        }

        //selectedItem = null;
        RefreshButton();
    }

    private void RefreshButton()
    {
        if (actionButton == null)
            return;

        bool hasItem = selectedItem != null;
        bool isHotbar = hasItem &&
            selectedItem.GetInventory() ==
            InventoryController.HotBarInventoryName;

        bool canEquip = hasItem &&
            selectedItem.GetEquipmentType() != EquipmentType.None &&
            !selectedItem.GetEquit();

        bool IsEquip = hasItem && 
            selectedItem.GetEquit();

        actionButton.interactable =
                isHotbar || IsEquip || canEquip;

        if (actionTextEvent != null)
        {
            actionTextEvent.StringReference =
                isHotbar ? unequipText : equipText;

            if(IsEquip)
                actionTextEvent.StringReference = unequipText;

            actionTextEvent.RefreshString();
        }
    }
}