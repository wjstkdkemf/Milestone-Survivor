using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 아이템 유형을 쉽게 구분하기 위한 열거형(enum)
public enum ItemType
{
    Equipment, // 장비
    Consumable, // 소모품
    Etc // 기타
}

// 아이템 등급
public enum ItemGrade
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName; // 아이템 이름
    [TextArea]
    public string description; // 아이템 설명
    public Sprite icon; // 아이콘 이미지
    public ItemType itemType; // 아이템 유형
    public ItemGrade grade; // 아이템 등급

    [Header("인벤토리 정보")]
    public int maxStackAmount = 1; // 최대 중첩 개수
    public bool displayItemAmount = true; // 개수 표시 여부
    public bool draggable = true; // 드래그 가능 여부
    public bool pressable = true; // 클릭 가능 여부
    public GameObject RelatedGameObject; // 관련된 게임 오브젝트
    public InventorySystem.InventoryItemEvent itemAction; // 아이템 사용 시 발생하는 이벤트
}
