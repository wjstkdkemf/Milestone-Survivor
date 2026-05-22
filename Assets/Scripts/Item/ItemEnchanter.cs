
using UnityEngine;
using InventorySystem;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using System;
using System.Collections;

public class ItemEnchanter : MonoBehaviour
{
    public static ItemEnchanter Instance { get; private set; }

    [Tooltip("The item currently selected for enchantment.")]
    private InventoryItem currentItem;

    [Tooltip("The UI button for enchanting.")]
    public Button enchantButton;
    //public TMP_Text MyGoldText;
    public TextMeshProUGUI enchantCostText;
    [SerializeField]private ItemStatContainer[] BeforeAfterStatPrefab;
    [SerializeField]private ItemLevelContainer BeforeAfterLevelPrefab;
    [SerializeField]private ItemUIEffectManager itemUIEffectManager;

    private bool isEnchanting;
    private bool IsMax = false;

    // 강화 레벨별 비용 (Key: 현재 레벨, Value: 다음 레벨로 가기 위한 비용)
    [System.Serializable]
    public class EnhancementRule
    {
        public int cost;
        [Range(0f, 1f)] public float successChance;
    }
    private readonly Dictionary<ItemGrade, Dictionary<int, EnhancementRule>> enhancementRulesByGrade = new Dictionary<ItemGrade, Dictionary<int, EnhancementRule>>
    {
        {ItemGrade.Common, new Dictionary<int, EnhancementRule>{
        {0, new EnhancementRule { cost = 10, successChance = 0.95f }},
        {1, new EnhancementRule { cost = 20, successChance = 0.85f }},
        {2, new EnhancementRule { cost = 30, successChance = 0.70f }},
        {3, new EnhancementRule { cost = 40, successChance = 0.55f }},
        {4, new EnhancementRule { cost = 50, successChance = 0.40f }},}}
    };
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
    private void OnDestroy()
    {
        // 내가 현재 활성화된 인스턴스였다면, 파괴될 때 참조를 비워줌
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (enchantButton != null)
        {
            enchantButton.onClick.AddListener(EnchantSelectedItem);
            enchantButton.interactable = false; // Initially disable the button
        }
        ReSetStatPrefab();
        ResetLevelPrefab();
    }

    public void SetItem(InventoryItem item)
    {
        currentItem = item;
        IsMax = false;
        UpdateEnchantButton();
        UpdateItemInfoDisplay();
    }

    private void UpdateEnchantButton()
    {
        if (enchantButton == null) return;

        if (currentItem == null || currentItem.GetIsNull())
        {
            enchantButton.interactable = false;
            enchantCostText.text = "";
            return;
        }

        if (enhancementCostsByGrade.TryGetValue(currentItem.GetGrade(), out var enhancementCosts))
        {
            if (enhancementCosts.TryGetValue(currentItem.GetEnhancementLevel(), out int cost))
            {
                if (enchantCostText != null)
                {
                    enchantCostText.text = cost.ToString();
                }
                enchantButton.interactable = PlayerStats.Instance.GoldAmount >= cost;
            }
            else
            {
                if (enchantCostText != null)
                {
                    enchantCostText.text = "";
                }
                IsMax = true;
                enchantButton.interactable = false;
            }
        }
        else
        {
            if (enchantCostText != null)
            {
                enchantCostText.text = "";
            }
            enchantButton.interactable = false;
        }
    }
    private void UpdateItemInfoDisplay()
    {
        ReSetStatPrefab();
        ResetLevelPrefab();
        if (currentItem == null || currentItem.GetIsNull())
        {
            return;
        }
        BeforeAfterLevelPrefab.SetLevelImage(currentItem.GetEnhancementLevel(),IsMax);
        int i = 0;

        if(currentItem.GetStatModifiers() != null)
        {
            foreach (var item in currentItem.GetStatModifiers())
            {
                BeforeAfterStatPrefab[i].SetStatImage(item, currentItem.GetEnhancementLevel(),IsMax);
                i++;
            }
        }
    }
    private void ReSetStatPrefab()
    {
        foreach(var con in BeforeAfterStatPrefab)
        {
            con.ResetData();
        }
    }
    private void ResetLevelPrefab()
    {
        BeforeAfterLevelPrefab.ResetData();
    }
    public void EnchantSelectedItem()
    {
        if (!isEnchanting)
        {
            StartCoroutine(EnchantRoutine());
        }
    }
    private IEnumerator EnchantRoutine()
    {
        if (currentItem == null || currentItem.GetIsNull()) yield break;

        int currentLevel = currentItem.GetEnhancementLevel();
        ItemGrade grade = currentItem.GetGrade();

        if (!TryGetEnhancementRule(grade, currentLevel, out EnhancementRule rule))
        {
            Debug.Log("[ItemEnchanter] Max level or no rule.");
            yield break;
        }

        if (!PlayerStats.Instance.TrySpendGold(rule.cost))
        {
            Debug.LogWarning("[ItemEnchanter] Not enough gold.");
            yield break;
        }

        isEnchanting = true;
        enchantButton.interactable = false;

        itemUIEffectManager?.OnEnhanceTry();

        yield return new WaitForSecondsRealtime(0.35f);

        bool success = UnityEngine.Random.value <= rule.successChance;

        if (success)
        {
            currentItem.SetEnhancementLevel(currentLevel + 1);
            HandleEnchantSync(currentItem);

            itemUIEffectManager?.OnEnhanceSuccess();
            Debug.Log($"'{currentItem.GetItemType()}' enhanced to +{currentItem.GetEnhancementLevel()}.");
        }
        else
        {
            itemUIEffectManager?.OnEnhanceFailure();
            Debug.Log($"'{currentItem.GetItemType()}' enhancement failed.");
        }

        LoadScreenManager.Instance.ConfirmSelectionSave();
        InventoryEventSystem.RaiseSlotClicked(currentItem, currentItem.GetInventory());

        UpdateItemInfoDisplay();
        UpdateEnchantButton();

        isEnchanting = false;
    }
    private bool TryGetEnhancementRule(ItemGrade grade,int currentLevel, out EnhancementRule rule)
    {
        rule = null;

        if (!enhancementRulesByGrade.TryGetValue(grade, out var rulesByLevel))
        {
            Debug.LogWarning($"[ItemEnchanter] No enhancement rules for grade: {grade}");
            return false;
        }

        if (!rulesByLevel.TryGetValue(currentLevel, out rule))
        {
            Debug.Log($"[ItemEnchanter] No enhancement rule for {grade} +{currentLevel}. Item may be max level.");
            return false;
        }

        return true;
    }
    /*
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
                if (PlayerStats.Instance.TrySpendGold(cost))
                {

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
                    }//
                    HandleEnchantSync(currentItem);

                    Debug.Log($"'{currentItem.GetItemType()}' successfully enchanted to +{currentItem.GetEnhancementLevel()}!");
                    LoadScreenManager.Instance.ConfirmSelectionSave();

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
    */

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
