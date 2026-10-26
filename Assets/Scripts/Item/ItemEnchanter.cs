
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

    private InventoryItem enchantHoldItem;

    [Tooltip("The UI button for enchanting.")]
    public Button enchantButton;
    //public TMP_Text MyGoldText;
    public TextMeshProUGUI enchantCostText;
    [SerializeField]private ItemStatContainer[] BeforeAfterStatPrefab;
    [SerializeField]private ItemLevelContainer BeforeAfterLevelPrefab;
    [SerializeField]private ItemUIEffectManager itemUIEffectManager;
    [SerializeField]private GameObject blockPanel;

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

        int currentLevel = currentItem.GetEnhancementLevel();
        ItemGrade grade = currentItem.GetGrade();

        bool hasGradeRules = enhancementRulesByGrade.TryGetValue(grade, out var rules);

        IsMax = hasGradeRules &&
                rules.Count > 0 &&
                currentLevel >= rules.Count;

        bool hasRule = TryGetEnhancementRule(
            grade, currentLevel, out EnhancementRule rule);

        if (IsMax || !hasRule)
        {
            if (enchantCostText != null)
                enchantCostText.text = "";

            enchantButton.interactable = false;
            return;
        }

        if (enchantCostText != null)
            enchantCostText.text = rule.cost.ToString();

        enchantButton.interactable =
            !isEnchanting &&
            PlayerStats.Instance != null &&
            PlayerStats.Instance.GoldAmount >= rule.cost;
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
        blockPanel.SetActive(true);

        enchantHoldItem = currentItem;

        itemUIEffectManager?.OnEnhanceTry();

        yield return new WaitForSecondsRealtime(0.35f);

        bool success = UnityEngine.Random.value <= rule.successChance;

        if (success)
        {
            enchantHoldItem.SetEnhancementLevel(currentLevel + 1);
            InventoryController.instance.HandleEnchantSync(enchantHoldItem);

            itemUIEffectManager?.OnEnhanceSuccess();
            Debug.Log($"'{enchantHoldItem.GetItemType()}' enhanced to +{enchantHoldItem.GetEnhancementLevel()}.");
        }
        else
        {
            itemUIEffectManager?.OnEnhanceFailure();
            Debug.Log($"'{enchantHoldItem.GetItemType()}' enhancement failed.");
        }

        LoadScreenManager.Instance.ConfirmSelectionSave();
        InventoryEventSystem.RaiseSlotClicked(enchantHoldItem, enchantHoldItem.GetInventory());

        enchantHoldItem = null;
        isEnchanting = false;

        UpdateEnchantButton();
        UpdateItemInfoDisplay();

        blockPanel.SetActive(false);
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
}
