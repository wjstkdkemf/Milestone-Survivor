
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
                    /*if (currentItem.GetEquit())
                    {
                        EquipmentData data = Resources.Load<EquipmentData>($"Items/{currentItem.GetItemType()}");
                        if (data != null)
                        {
                            // Unequip with old stats and re-equip with new stats
                            EquipmentEffectManager.Instance.Unequip(data, currentItem);
                            EquipmentEffectManager.Instance.Equip(data, currentItem);
                        }
                    }*/
                    HandleEnchantSync(currentItem);

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
    /// <summary>
    /// 강화 후 장착 상태에 따라 스탯을 갱신하고 데이터를 동기화합니다.
    /// </summary>
    private void HandleEnchantSync(InventoryItem enchantedItem)
    {
        bool isEquipped = enchantedItem.GetEquit();
        string invName = enchantedItem.GetInventory();

        var controller = InventoryController.instance;
        var uiManager = controller.GetInventoryUIByName(invName);
        if (uiManager != null)
        {
            uiManager.UpdateSlot(enchantedItem.GetPosition());
        }

        // Case A: 인벤토리에서 강화했는데 장착 중인 경우
        if (isEquipped)
        {
            // 만약 '인벤토리'에서 강화했다면 -> '장착창(HotBar)'의 아이템도 레벨업 필요
            SyncToEquipmentSlot(enchantedItem);
        }
        else if(invName == "HotBar") // 장착 중인 아이템을 직접 강화한 경우
        {
            // 만약 '장착창(HotBar)'에서 강화했다면 -> '인벤토리(Inventory)'의 원본도 레벨업 필요
            SyncToMainInventory(enchantedItem);
        }
        // 플레이어 스탯 즉시 재계산 (굳이 뺐다 낄 필요 없음)
        RecalculatePlayerStats();
    }
    /// <summary>
    /// 장착 장비의 변화를 감지하여 플레이어 스탯을 재계산합니다.
    /// </summary>
    private void RecalculatePlayerStats()
    {
        // 기존의 Unequip -> Equip 대신, 현재 장착된 모든 아이템을 기준으로 스탯을 다시 계산합니다.
        // InventoryController에 이미 있는 기능을 활용하거나 새로 만듭니다.
        if (InventoryController.instance != null)
        {
            InventoryController.instance.ReapplyAllEquipmentEffects();
        }
    }

    /// <summary>
    /// 장착창(HotBar)에서 강화된 내용을 메인 인벤토리(Inventory)에 반영합니다.
    /// </summary>
    private void SyncToMainInventory(InventoryItem equippedItem)
    {
        var controller = InventoryController.instance;
        var mainInv = controller.GetInventory("Inventory");
        
        // 메인 인벤토리에서 같은 타입의 아이템을 찾음
        var originalItem = mainInv.GetList().Find(x => 
            x != null && !x.GetIsNull() && x.GetItemType() == equippedItem.GetItemType());

        if (originalItem != null)
        {
            originalItem.SetEnhancementLevel(equippedItem.GetEnhancementLevel());
            // UI 갱신
            var uiManager = controller.GetInventoryUIByName("Inventory");
            if (uiManager != null)
            {
                uiManager.UpdateSlot(originalItem.GetPosition());
            }
            Debug.Log("[Sync] 메인 인벤토리 아이템 동기화 완료");
        }
        else
            Debug.Log("서칭실패");
    }

    /// <summary>
    /// 메인 인벤토리에서 강화된 내용을 장착창(HotBar)에 반영합니다.
    /// </summary>
    private void SyncToEquipmentSlot(InventoryItem inventoryItem)
    {
        var controller = InventoryController.instance;
        var equipInv = controller.GetInventory("HotBar");

        // 장착창에서 같은 타입의 아이템을 찾음
        var equippedItem = equipInv.GetList().Find(x => 
            x != null && !x.GetIsNull() && x.GetItemType() == inventoryItem.GetItemType());

        if (equippedItem != null)
        {
            equippedItem.SetEnhancementLevel(inventoryItem.GetEnhancementLevel());
            
            // 장착창 UI 갱신 (레벨 숫자가 바뀌었을 테니)
            var uiManager = controller.GetInventoryUIByName("HotBar");
            if (uiManager != null)
            {
                uiManager.UpdateSlot(equippedItem.GetPosition());
            }
            Debug.Log("[Sync] 장착창 아이템 동기화 완료");
        }
        else
            Debug.Log("서칭실패");
    }
}
