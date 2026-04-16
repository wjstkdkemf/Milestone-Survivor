using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

[System.Serializable]
public class QuestProgressData
{
    public string questID;
    public List<int> currentCounts; // 다중 조건의 현재 진행도를 순서대로 저장
    public bool isCompleted;

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
            LoadQuestData();
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

public void AssignRandomSubQuests(string regionID)
    {
        currentSubQuests.Clear();

        var pool = questDatabase.Where(q => 
            q.regionID == regionID && 
            !q.isMainQuest && 
            !GameProgressManager.Instance.IsUnlocked($"Cleared_{q.questID}")
        ).ToList();

        for (int i = 0; i < 2; i++)
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
                
                // selected.rarity 값에 따라 여기서 UI 연출
                Debug.Log($"[{selected.rarity}] 등급 퀘스트 당첨: {selected.questName}");
            }
        }

        SaveQuestData();
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

        Debug.Log($"💰 보상 획득: {data.rewardGold} G");

        GameProgressManager.Instance.Unlock($"Cleared_{data.questID}");
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
    }

    public QuestDataSO GetQuestSO(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return questDatabase.FirstOrDefault(q => q.questID == id);
    }
}