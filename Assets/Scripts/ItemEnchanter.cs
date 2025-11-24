
using UnityEngine;
using InventorySystem;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ItemEnchanter : MonoBehaviour
{
    public static ItemEnchanter Instance { get; private set; }

    [Tooltip("The item currently selected for enchantment.")]
    private InventoryItem currentItem;

    [Tooltip("The UI button for enchanting.")]
    public Button enchantButton;
    public TMP_Text MyGoldText;
    public TextMeshProUGUI enchantText;

    // 강화 레벨별 비용 (Key: 현재 레벨, Value: 다음 레벨로 가기 위한 비용)
    private readonly Dictionary<ItemGrade, Dictionary<int, int>> enhancementCostsByGrade = new Dictionary<ItemGrade, Dictionary<int, int>>
    {
        { ItemGrade.Common, new Dictionary<int, int> { {0, 10}, {1, 20}, {2, 30}, {3, 40}, {4, 50} } },
        { ItemGrade.Uncommon, new Dictionary<int, int> { {0, 100}, {1, 200}, {2, 300}, {3, 400}, {4, 500} } },
        { ItemGrade.Rare, new Dictionary<int, int> { {0, 1000}, {1, 2000}, {2, 3000}, {3, 4000}, {4, 5000} } },
        { ItemGrade.Epic, new Dictionary<int, int> { {0, 5000}, {1, 10000}, {2, 15000}, {3, 20000}, {4, 25000} } },
        { ItemGrade.Legendary, new Dictionary<int, int> { {0, 10000}, {1, 20000}, {2, 30000}, {3, 40000}, {4, 50000} } }
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

    private void OnEnable()
    {
        PlayerStats.OnGoldChanged += HandleGoldChanged;
        UpdateGold();
    }

    private void OnDisable()
    {
        PlayerStats.OnGoldChanged -= HandleGoldChanged;
    }

    private void HandleGoldChanged(int newGoldAmount)
    {
        UpdateGold();
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
    private void UpdateGold()
    {
        if(MyGoldText != null)
        {
            string GoldText_Format = PlayerStats.Instance.Format(PlayerStats.Instance.GoldAmount);
            MyGoldText.text = $"{GoldText_Format}";
        } 
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

        if (enhancementCostsByGrade.TryGetValue(currentItem.GetGrade(), out var enhancementCosts))
        {
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
        else
        {
            if (enchantText != null)
            {
                enchantText.text = "";
            }
            // This grade is not in the cost dictionary
            enchantButton.interactable = false;
        }

        UpdateGold();
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
        ItemGrade currentGrade = currentItem.GetGrade();

        if (enhancementCostsByGrade.TryGetValue(currentGrade, out var enhancementCosts))
        {
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
        else
        {
            Debug.Log($"[ItemEnchanter] Enhancement cost for grade '{currentGrade}' is not defined.");
        }
    }
}
