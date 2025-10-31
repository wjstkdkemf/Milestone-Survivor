
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;
    public string startPointName = "StartPoint";
    public string startMapName = "StartMap";

    [Header("중앙 데이터베이스")]
    public TeleportDatabase database; // 인스펙터에서 만든 Database 에셋을 연결
    [Header("플레이어 진행도")]
    private TeleportSaveData saveData; // 세이브 파일에서 불러온 실제 잠금 해제 현황

    private List<TeleportPoint> teleportPoints;
    private string saveFilePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            teleportPoints = new List<TeleportPoint>();
            saveFilePath = Path.Combine(Application.persistentDataPath, "teleport.json");
            LoadData();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Start()
    {
        
    }
    public void RegisterPoint(TeleportPoint point)
    {
        Debug.Log(point.teleportPointName);
        if(!IsPointUnlocked(point.teleportPointName))
        {
            Debug.Log(point.teleportPointName);
            UnlockPoint(point.teleportPointName);
        }
        teleportPoints.Add(point);
    }
    public void UnRegisterPoint(TeleportPoint point)
    {
        teleportPoints.Remove(point);
    }
    public void ResetPoints()
    {
        teleportPoints.Clear();
    }

    public void TeleportPlayer(GameObject player,string newMapName ,string teleportPointName)
    {
        TeleportPoint destination = teleportPoints.FirstOrDefault(point => point.teleportPointName == teleportPointName);

        if (destination != null)
        {
            player.transform.position = destination.transform.position;
        }
        else
        {
            MainMapManager.Instance.ChangeMap(newMapName, teleportPointName);
        }
    }

    public void SetInitialSpawnPoint()//GameObject player
    {
        MainMapManager.Instance.ChangeMap(startMapName, startPointName);
        //TeleportPlayer(player, startPointName);
    }

    public List<TeleportPoint> GetTeleportPoints()
    {
        return teleportPoints;
    }

    public void SetName(string Mapname, string pointName)//버튼에 넣을것
    {
        startMapName = Mapname;
        startPointName = pointName;
    }

    // --- UI가 호출할 함수 ---
    public List<TeleportZoneData> GetAllTeleportData()
    {
        // 텔레포트 UI가 이 함수를 호출해 '모든' 포인트 목록을 받아갑니다.
        return database.allZoneGroups;
    }
    public bool IsPointUnlocked(string pointID)
    {
        // 1. ScriptableObject의 기본 잠금 해제 상태 확인
        // (데이터 에셋 자체의 isUnlocked가 true라면 기본 해제된 것)
        TeleportData data = database.allZoneGroups
                            .SelectMany(group => group.pointsInZone)
                            .FirstOrDefault(point => point.targetSpawnPointID == pointID);
        if (data != null && data.isUnlocked)
        {
            return true;
        }
        Debug.Log(saveData.unlockedPoints.ContainsKey(pointID) + "  " + pointID);

        // 2. 세이브 파일(saveData)에서 플레이어가 해제했는지 확인
        return saveData.unlockedPoints.ContainsKey(pointID) && saveData.unlockedPoints[pointID];
    }
    public void UnlockPoint(string pointID)
    {
        if (!IsPointUnlocked(pointID))
        {
            saveData.unlockedPoints[pointID] = true;
            SaveData(); // 상태 변경 후 즉시 파일로 저장
            Debug.Log($"포인트 잠금 해제 및 저장: {pointID}");
        }
    }

    public void SaveTeleportData(TeleportSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("텔레포트 데이터 저장 완료.");
    }

    // TeleportManager가 호출할 불러오기 함수
    public TeleportSaveData LoadTeleportData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            TeleportSaveData data = JsonUtility.FromJson<TeleportSaveData>(json);
            Debug.Log("텔레포트 데이터 로드 완료.");
            return data;
        }
        else
        {
            // 세이브 파일이 없으면 새로 생성
            Debug.Log("세이브 파일 없음. 새 데이터 생성.");
            return new TeleportSaveData();
        }
    }
    private void SaveData()
    {
       SaveTeleportData(saveData);
    }

    private void LoadData()
    {
        saveData = LoadTeleportData();
    }
}
