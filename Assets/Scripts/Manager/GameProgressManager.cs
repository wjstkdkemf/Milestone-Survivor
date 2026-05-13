using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

// 게임의 영구적인 진행 상태(업적, 보스 클리어, 퀘스트 완료 등)를 저장합니다.
public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }
    private const int CurrentProgressSaveVersion = 1;

    // 고유 ID를 저장하는 HashSet. "Boss_Forest_Defeated" "ACH_001" 등
    // List<T>보다 IsUnlocked() 체크가 훨씬 빠릅니다.
    private HashSet<string> unlockedProgress = new HashSet<string>();

    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "testprogress.json");//progress.json
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 게임 종료 시 자동 저장을 위한 로직
    private void OnApplicationQuit()
    {
        //지금은 SaveLoadManager.cs에서 진행중
        //SaveProgress();
    }

    // 새로운 진행 상태 '깃발'을 등록합니다.
    public void Unlock(string progressID)
    {
        if (unlockedProgress.Contains(progressID))
        {
            Debug.Log($"Progress '{progressID}'는 이미 달성되었습니다.");
            return;
        }

        unlockedProgress.Add(progressID);
        Debug.Log($"Progress 달성: {progressID}");
        
        // 업적 달성 시 바로 저장하는 것도 좋은 방법입니다.
        SaveProgress();
    }
    public void Dislock(string progressID)
    {
        if (unlockedProgress.Contains(progressID))
        {
            unlockedProgress.Remove(progressID);
            Debug.Log($"Progress 제거: {progressID}");
            SaveProgress();
        }
    }

    // 해당 '깃발'이 등록되어 있는지 확인합니다.
    public bool IsUnlocked(string progressID)
    {
        return unlockedProgress.Contains(progressID);
    }

    // 진행 상태를 초기화할 수도 있습니다.
    public void ResetProgress()
    {
        unlockedProgress.Clear();
        SaveProgress();
        Debug.Log("모든 진행 상태가 초기화되었습니다.");
    }

    // 진행 상태를 JSON 파일로 저장
    public void SaveProgress()
    {
        // HashSet은 JsonUtility로 직접 저장이 안되므로 List로 변환
        List<string> progressList = unlockedProgress.ToList();
        string json = JsonUtility.ToJson(new Serialization<string>(progressList, CurrentProgressSaveVersion, Application.version));
        File.WriteAllText(savePath, json);
        Debug.Log("게임 진행도 저장 완료: " + savePath);
    }

    // JSON 파일에서 진행 상태 불러오기
    public void LoadProgress()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            Serialization<string> data = null;

            try
            {
                data = JsonUtility.FromJson<Serialization<string>>(json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[GameProgressManager] Failed to parse progress save data: {exception.Message}");
            }

            if (data == null)
            {
                Debug.LogWarning("[GameProgressManager] Progress save data is invalid. Starting with empty progress.");
                unlockedProgress.Clear();
                return;
            }

            RepairProgressSaveData(data);

            // List를 다시 HashSet으로 변환하여 로드
            unlockedProgress = new HashSet<string>(data.items);
            Debug.Log("게임 진행도 불러오기 완료.");
        }
        else
        {
            Debug.Log("저장된 진행도 파일이 없습니다. 새 게임입니다.");
        }
    }

    private void RepairProgressSaveData(Serialization<string> data)
    {
        if (data.saveVersion <= 0)
        {
            data.saveVersion = 1;
            data.appVersion = string.IsNullOrEmpty(data.appVersion) ? "Legacy" : data.appVersion;
        }

        if (data.saveVersion > CurrentProgressSaveVersion)
        {
            Debug.LogWarning($"[GameProgressManager] Progress save version {data.saveVersion} is newer than supported version {CurrentProgressSaveVersion}. Loading with best effort.");
        }

        if (data.items == null)
        {
            data.items = new List<string>();
        }
    }
}

// JsonUtility가 List는 변환하지만 Dictionary/HashSet은 직접 변환하지 못해, 감싸주는 클래스입니다.
// InventoryManager에서 사용했던 것과 동일합니다.
[Serializable]
public class Serialization<T>
{
    public int saveVersion;
    public string appVersion;
    public List<T> items;

    public Serialization()
    {
        items = new List<T>();
    }

    public Serialization(List<T> items) => this.items = items;

    public Serialization(List<T> items, int saveVersion, string appVersion)
    {
        this.items = items;
        this.saveVersion = saveVersion;
        this.appVersion = appVersion;
    }
}
