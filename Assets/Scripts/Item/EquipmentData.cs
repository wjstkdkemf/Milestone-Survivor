using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 장비 부위
public enum EquipmentType
{
    Helmet, // 투구
    Armor, // 갑옷
    Gloves, // 장갑
    Boots, // 신발
    Accessory // 장신구
}

[CreateAssetMenu(fileName = "New Equipment Data", menuName = "Data/Equipment")]
public class EquipmentData : ItemData
{
    [Header("장비 정보")]
    public EquipmentType equipmentType; // 장비 부위
    public List<StatModifier> statModifiers; // 스탯 옵션 리스트
}

// 스탯 옵션을 표현하기 위한 간단한 구조체
[System.Serializable]
public struct StatModifier
{
    public string statName; // 예: "최대 체력", "공격력"
    public float value;
}

