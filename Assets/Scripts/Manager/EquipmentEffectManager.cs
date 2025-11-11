using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentEffectManager : MonoBehaviour
{
    #region Singleton
    private static EquipmentEffectManager instance;
    public static EquipmentEffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EquipmentEffectManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("EquipmentEffectManager");
                    instance = obj.AddComponent<EquipmentEffectManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    // 각 장비의 고유 ID(예: 인스턴스 ID 또는 이름)를 키로, 적용된 스탯 리스트를 값으로 저장
    private readonly Dictionary<string, List<StatModifier>> equippedItemStats = new Dictionary<string, List<StatModifier>>();
    private readonly Dictionary<int, float> enhancementMultipliers = new Dictionary<int, float>
    {
        {0, 1.0f}, {1, 1.1f}, {2, 1.2f}, {3, 1.4f}, {4, 1.6f}, {5, 2.0f}
        // 필요에 따라 강화 레벨과 배율을 추가하세요.
    };
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// 장비를 장착하여 강화 레벨에 따른 스탯 보너스를 추가합니다.
    /// </summary>
    /// <param name="equipment">장착할 장비의 EquipmentData</param>
    /// <param name="itemInstance">장착할 아이템의 인스턴스 정보</param>
    public void Equip(EquipmentData equipment, InventorySystem.InventoryItem itemInstance)
    {
        if (equipment == null || itemInstance == null) return;

        string itemKey = itemInstance.GetItemType(); // 우선 아이템 이름으로 고유성을 확보, 필요시 ID 사용
        if (equippedItemStats.ContainsKey(itemKey))
        {
            Debug.LogWarning($"[EquipmentEffectManager] Item '{itemKey}' is already equipped. Unequipping before re-equipping.");
            Unequip(equipment, itemInstance);
        }

        int enhancementLevel = itemInstance.GetEnhancementLevel();
        enhancementMultipliers.TryGetValue(enhancementLevel, out float multiplier);
        if (multiplier == 0) multiplier = 1.0f; // 배율이 정의되지 않은 경우 기본값 1.0

        List<StatModifier> appliedModifiers = new List<StatModifier>();
        foreach (var modifier in equipment.statModifiers)
        {
            StatModifier newModifier = new StatModifier
            {
                statName = modifier.statName,
                value = modifier.value * multiplier
            };
            appliedModifiers.Add(newModifier);
            Debug.Log($"Equipped {equipment.itemName} (Lv.{enhancementLevel}): {newModifier.statName} +{newModifier.value} (Multiplier: {multiplier}x)");
        }

        equippedItemStats[itemKey] = appliedModifiers;
    }

    /// <summary>
    /// 장비를 해제하여 스탯 보너스를 제거합니다.
    /// </summary>
    /// <param name="equipment">해제할 장비의 EquipmentData</param>
    /// <param name="itemInstance">해제할 아이템의 인스턴스 정보</param>
    public void Unequip(EquipmentData equipment, InventorySystem.InventoryItem itemInstance)
    {
        if (equipment == null || itemInstance == null) return;

        string itemKey = itemInstance.GetItemType();
        if (equippedItemStats.Remove(itemKey))
        {
            Debug.Log($"Unequipped {equipment.itemName}.");
        }
    }

    /// <summary>
    /// 특정 스탯의 총 보너스 값을 가져옵니다.
    /// </summary>
    /// <param name="statName">가져올 스탯의 이름</param>
    /// <returns>스탯 보너스의 총합, 해당 스탯이 없으면 0을 반환합니다.</returns>
    public float GetStatBonus(string statName)
    {
        float totalBonus = 0;
        foreach (var itemList in equippedItemStats.Values)
        {
            foreach (var modifier in itemList)
            {
                if (modifier.statName == statName)
                {
                    totalBonus += modifier.value;
                }
            }
        }
        return totalBonus;
    }

    /// <summary>
    /// 현재 적용된 모든 스탯 보너스를 초기화합니다.
    /// </summary>
    public void ClearAllEffects()
    {
        equippedItemStats.Clear();
        Debug.Log("[EquipmentEffectManager] All stat modifiers cleared.");
    }
}
