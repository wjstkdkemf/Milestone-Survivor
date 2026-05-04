using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using InventorySystem;

[System.Serializable]
public class QuestProgressData
{
    public string questID;
    public List<int> currentCounts; // 다중 조건의 현재 진행도를 순서대로 저장
    public bool isCompleted;
    public bool isClaimed;

    public void InitializeCounts(int conditionCount)
    {
        currentCounts = new List<int>(new int[conditionCount]);
    }
}

[System.Serializable]
public class QuestSaveData
{
    public QuestProgressData mainQuest;
    public List<QuestProgressData> activeSubQuests = new List<QuestProgressData>();
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Database")]
    public List<QuestDataSO> questDatabase; // 게임 내 모든 퀘스트 SO 리스트를 드래그 앤 드롭

    [Header("Current Progress Slots")]
    public string currentRegionID = "Village"; // 현재 지역 ID (반복 퀘스트 할당용)
    public QuestProgressData currentMainQuest;
    public List<QuestProgressData> currentSubQuests = new List<QuestProgressData>();

    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            saveFilePath = Path.Combine(Application.persistentDataPath, "quest_progress.json");
            DontDestroyOnLoad(gameObject);
            //LoadQuestData();
        }
        else Destroy(gameObject);
    }
    private void OnEnable()
    {
        GlobalEventManager.OnEnemyKilled += HandleEnemyKilled;
        GlobalEventManager.OnEncounterCleared += HandleEncounterCleared;
    }

    private void OnDisable()
    {
        GlobalEventManager.OnEnemyKilled -= HandleEnemyKilled;
        GlobalEventManager.OnEncounterCleared -= HandleEncounterCleared;
    }

    /// <summary>
    /// 현재 지역의 서브 퀘스트를 최대 개수(2개)까지 채웁니다.
    /// </summary>
    public void FillSubQuests()
    {
        int needed = 2 - currentSubQuests.Count;
        if (needed <= 0) return;

        var pool = questDatabase.Where(q => 
            q.regionID == currentRegionID && 
            !q.isMainQuest && 
            !GameProgressManager.Instance.IsUnlocked(q.questID) &&
            currentSubQuests.All(sq => sq.questID != q.questID) // 이미 진행 중인 퀘스트 제외
            ).ToList();

        for (int i = 0; i < needed; i++)
        {
            if (pool.Count == 0) break;

            int totalWeight = pool.Sum(q => q.weight);
            int randomValue = Random.Range(0, totalWeight);
            int currentWeightSum = 0;

            QuestDataSO selected = null;

            foreach (var q in pool)
            {
                currentWeightSum += q.weight;
                if (randomValue < currentWeightSum)
                {
                    selected = q;
                    break;
                }
            }

            if (selected != null)
            {
                QuestProgressData newQuest = new QuestProgressData {
                    questID = selected.questID,
                    isCompleted = false
                };
                newQuest.InitializeCounts(selected.conditions.Count);
                
                currentSubQuests.Add(newQuest);
                pool.Remove(selected); // 중복 당첨 방지
                
                Debug.Log($"[{selected.rarity}] 등급 퀘스트 할당: {selected.questName}");
            }
        }

        SaveQuestData();
    }

    public void AssignRandomSubQuests(string regionID)
    {
        currentRegionID = regionID;
        currentSubQuests.Clear();
        FillSubQuests();
    }

    public void AssignMainQuest(string questID)
    {
        QuestDataSO data = GetQuestSO(questID);
        if (data != null)
        {
            currentMainQuest = new QuestProgressData { questID = questID, isCompleted = false };
            currentMainQuest.InitializeCounts(data.conditions.Count);
            SaveQuestData();
        }
        else
        {
            Debug.LogError($"퀘스트 데이터 '{questID}'를 찾을 수 없습니다. DB를 확인하세요.");
        }
    }

    private void HandleEnemyKilled(string enemyID)
    {
        ProcessEvent(QuestType.KillEnemy, enemyID, currentMainQuest);

        foreach (var quest in currentSubQuests)
        {
            ProcessEvent(QuestType.KillEnemy, enemyID, quest);
        }
    }

    private void HandleEncounterCleared()
    {
        ProcessEvent(QuestType.ClearEncounter, "", currentMainQuest);
        foreach (var quest in currentSubQuests) ProcessEvent(QuestType.ClearEncounter, "", quest);
    }

    private void ProcessEvent(QuestType type, string targetID, QuestProgressData quest)
    {
        if (quest == null || quest.isCompleted) return;

        Debug.Log("호출 체크");

        QuestDataSO data = GetQuestSO(quest.questID);
        if (data == null) return;

        bool isUpdated = false;

        for (int i = 0; i < data.conditions.Count; i++)
        {
            var cond = data.conditions[i];
            
            if (cond.questType == type && (string.IsNullOrEmpty(cond.targetID) || cond.targetID == targetID))
            {
                if (quest.currentCounts[i] < cond.targetAmount)
                {
                    quest.currentCounts[i]++;
                    isUpdated = true;
                }
            }
        }

        if (isUpdated)
        {
            CheckCompletion(quest, data);
        }
    }

    private void CheckCompletion(QuestProgressData quest, QuestDataSO data)
    {
        bool allClear = true;

        for (int i = 0; i < data.conditions.Count; i++)
        {
            if (quest.currentCounts[i] < data.conditions[i].targetAmount)
            {
                allClear = false;
                break;
            }
        }

        if (allClear)
        {
            quest.isCompleted = true;
            Debug.Log($"🎉 퀘스트 완료 대기 중: {data.questName}");
        }
    }

    public void ClaimReward(QuestProgressData quest)
    {
        if (quest == null || !quest.isCompleted) return;

        QuestDataSO data = GetQuestSO(quest.questID);
        if (data == null) return;

        foreach(QuestReward reward in data.rewards)
        {
            if(reward.rewardType == RewardType.Gold)
            {
                PlayerStats.Instance.AddCoin(reward.amount);
            }
            else if(reward.rewardType == RewardType.Item)
            {
                ItemObject itemObject = reward.lootTable.QuestDrop().itemPrefab.GetComponent<ItemObject>();
                InventoryController.instance.AddItem("Inventory", itemObject.itemData.itemName, 1);
            }
        }
        //Debug.Log($"💰 보상 획득: {data.rewardGold} G");

        //GameProgressManager.Instance.Unlock($"Cleared_{data.questID}");
        if (!string.IsNullOrEmpty(data.unlockProgressID))
        {
            GameProgressManager.Instance.Unlock(data.unlockProgressID);
        }

        if (quest == currentMainQuest) currentMainQuest = null;
        else currentSubQuests.Remove(quest);

        SaveQuestData();
    }
    public void SaveQuestData()
    {
        QuestSaveData data = new QuestSaveData {
            mainQuest = currentMainQuest,
            activeSubQuests = currentSubQuests
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
    }

    public void LoadQuestData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            QuestSaveData data = JsonUtility.FromJson<QuestSaveData>(json);
            
            currentMainQuest = data.mainQuest;
            currentSubQuests = data.activeSubQuests;
        }

        if (currentMainQuest == null || string.IsNullOrEmpty(currentMainQuest.questID))
        {
            Debug.Log("초기 메인 퀘스트 할당 시도: Main_Test");
            AssignMainQuest("Main_Test");
        }
    }

    public QuestDataSO GetQuestSO(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return questDatabase.FirstOrDefault(q => q.questID == id);
    }
}