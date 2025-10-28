
using UnityEngine;
using InventorySystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemEnchanter : MonoBehaviour
{
    public static ItemEnchanter Instance { get; private set; }

    [Tooltip("The item currently selected for enchantment.")]
    private InventoryItem currentItem;

    [Tooltip("The UI button for enchanting.")]
    public Button enchantButton;
    public TMPro.TextMeshProUGUI enchantText;

    // 강화 레벨별 비용 (Key: 현재 레벨, Value: 다음 레벨로 가기 위한 비용)
    private readonly Dictionary<int, int> enhancementCosts = new Dictionary<int, int>
    {
        {0, 10},   // 0 -> 1
        {1, 250},   // 1 -> 2
        {2, 500},   // 2 -> 3
        {3, 1000},  // 3 -> 4
        {4, 2000}   // 4 -> 5
        // 필요에 따라 레벨과 비용을 추가하세요.
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (enchantButton != null)
        {
            enchantButton.onClick.AddListener(EnchantSelectedItem);
            enchantButton.interactable = false; // Initially disable the button
        }
    }

    /// <summary>
    /// Sets the item to be enchanted.
    /// </summary>
    public void SetItem(InventoryItem item)
    {
        currentItem = item;
        UpdateEnchantButton();
    }

    /// <summary>
    /// Updates the enchant button's state based on the current item and player's gold.
    /// </summary>
    private void UpdateEnchantButton()
    {
        if (enchantButton == null) return;

        if (currentItem == null || currentItem.GetIsNull())
        {
            enchantButton.interactable = false;
            return;
        }

        if (enhancementCosts.TryGetValue(currentItem.GetEnhancementLevel(), out int cost))
        {
            if (enchantText != null)
            {
                enchantText.text = cost.ToString();
            }
            // Player has enough gold AND the item is not max level
            enchantButton.interactable = PlayerStats.Instance.GoldAmount >= cost;
        }
        else
        {
            if (enchantText != null)
            {
                enchantText.text = "";
            }
            // This is the max level, or level is not in the cost dictionary
            enchantButton.interactable = false;
        }

    }

    /// <summary>
    /// Called by the enchant button's OnClick event.
    /// </summary>
    public void EnchantSelectedItem()
    {
        if (currentItem == null || currentItem.GetIsNull())
        {
            Debug.LogWarning("[ItemEnchanter] No item selected to enchant.");
            return;
        }

        int currentLevel = currentItem.GetEnhancementLevel();

        if (enhancementCosts.TryGetValue(currentLevel, out int cost))
        {
            if (PlayerStats.Instance.GoldAmount >= cost)
            {
                // Deduct gold
                PlayerStats.Instance.GoldAmount -= cost;

                // Increase enhancement level
                currentItem.SetEnhancementLevel(currentLevel + 1);

                // Re-apply equipment effects if the item is equipped
                if (currentItem.GetEquit())
                {
                    EquipmentData data = Resources.Load<EquipmentData>($"Items/{currentItem.GetItemType()}");
                    if (data != null)
                    {
                        // Unequip with old stats and re-equip with new stats
                        EquipmentEffectManager.Instance.Unequip(data, currentItem);
                        EquipmentEffectManager.Instance.Equip(data, currentItem);
                    }
                }

                Debug.Log($"'{currentItem.GetItemType()}' successfully enchanted to +{currentItem.GetEnhancementLevel()}!");

                // Update the UI (ItemInfoDisplay and Enchant Button)
                InventoryEventSystem.RaiseSlotClicked(currentItem, currentItem.GetInventory());
                UpdateEnchantButton();
            }
            else
            {
                Debug.LogWarning("[ItemEnchanter] Not enough gold to enchant.");
            }
        }
        else
        {
            Debug.Log("[ItemEnchanter] Item is at max level or enhancement cost is not defined for this level.");
        }
    }
}
