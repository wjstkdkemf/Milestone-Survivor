using System.Collections.Generic;
using UnityEngine;

public enum QuestType { KillEnemy, ClearEncounter, CollectItem }
public enum QuestRarity { Common, Rare, Epic, Legendary }
public enum RewardType { Gold, Item }

[System.Serializable]
public class QuestCondition
{
    public QuestType questType;
    public string targetID; 
    public int targetAmount;
}
[System.Serializable]
public class QuestReward
{
    public RewardType rewardType;
    public Sprite rewardIcon;   // 인스펙터에서 넣을 아이콘 (골드 이미지, 상자 이미지 등)
    
    [Header("Details")]
    public int amount;          // 골드나 경험치량
    public LootTable lootTable;     // 아이템일 경우 아이템 드랍 테이블.
}

[CreateAssetMenu(fileName = "New Quest Data", menuName = "System/Quest Data")]
public class QuestDataSO : ScriptableObject
{
    public string questID;
    public string questName;
    [TextArea] public string description;
    public string regionID;
    public bool isMainQuest;

    [Header("Requirements (다중 조건 가능)")]
    public List<QuestCondition> conditions = new List<QuestCondition>();
    [Header("Rarity & Weight")]
    public QuestRarity rarity;    // 등급 (연출용)
    public int weight = 100;      // 가중치 (기본값 100, 높을수록 잘 나옴)

    [Header("Rewards (다중 보상 가능)")]
    public List<QuestReward> rewards = new List<QuestReward>();
    [Tooltip("완료 시 GameProgressManager에 등록될 업적 ID (옵션)")]
    public string unlockProgressID;//메인퀘스트용.
}