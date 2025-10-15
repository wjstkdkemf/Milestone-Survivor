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

    private readonly Dictionary<string, float> currentStatModifiers = new Dictionary<string, float>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 장비를 장착하여 스탯 보너스를 추가합니다.
    /// </summary>
    /// <param name="equipment">장착할 장비의 EquipmentData</param>
    public void Equip(EquipmentData equipment)
    {
        if (equipment == null) return;

        foreach (var modifier in equipment.statModifiers)
        {
            if (currentStatModifiers.ContainsKey(modifier.statName))
            {
                currentStatModifiers[modifier.statName] += modifier.value;
            }
            else
            {
                currentStatModifiers[modifier.statName] = modifier.value;
            }
            Debug.Log($"Equipped {equipment.itemName}: {modifier.statName} +{modifier.value}. Total bonus: {currentStatModifiers[modifier.statName]}");
        }
    }

    /// <summary>
    /// 장비를 해제하여 스탯 보너스를 제거합니다.
    /// </summary>
    /// <param name="equipment">해제할 장비의 EquipmentData</param>
    public void Unequip(EquipmentData equipment)
    {
        if (equipment == null) return;

        foreach (var modifier in equipment.statModifiers)
        {
            if (currentStatModifiers.ContainsKey(modifier.statName))
            {
                currentStatModifiers[modifier.statName] -= modifier.value;
                Debug.Log($"Unequipped {equipment.itemName}: {modifier.statName} -{modifier.value}. Total bonus: {currentStatModifiers[modifier.statName]}");
            }
        }
    }

    /// <summary>
    /// 특정 스탯의 총 보너스 값을 가져옵니다.
    /// </summary>
    /// <param name="statName">가져올 스탯의 이름</param>
    /// <returns>스탯 보너스의 총합, 해당 스탯이 없으면 0을 반환합니다.</returns>
    public float GetStatBonus(string statName)
    {
        currentStatModifiers.TryGetValue(statName, out float value);
        return value;
    }

    /// <summary>
    /// 현재 적용된 모든 스탯 보너스를 초기화합니다.
    /// </summary>
    public void ClearAllEffects()
    {
        currentStatModifiers.Clear();
        Debug.Log("[EquipmentEffectManager] All stat modifiers cleared.");
    }
}
