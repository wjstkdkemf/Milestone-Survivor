using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class UpgradeChanceData
{
    public UpgradeScriptableObject.UpgardeEnum upgradeType;
    public int chance;
}

[System.Serializable]
public class UpgradeChanceDataList
{
    public List<UpgradeChanceData> chances = new List<UpgradeChanceData>();
}

[System.Serializable]
public class UpgradePointData
{
    public UpgradeScriptableObject.UpgardeEnum upgradeType;
    public int points;
}

[System.Serializable]
public class UpgradePointDataList
{
    public List<UpgradePointData> points = new List<UpgradePointData>();
}

public class PersistentDataManager : MonoBehaviour
{
    public static PersistentDataManager Instance;

    public Dictionary<UpgradeScriptableObject.UpgardeEnum, int> initialUpgradeChances = new Dictionary<UpgradeScriptableObject.UpgardeEnum, int>();
    public Dictionary<UpgradeScriptableObject.UpgardeEnum, int> currentUpgradeChances = new Dictionary<UpgradeScriptableObject.UpgardeEnum, int>();
    public Dictionary<UpgradeScriptableObject.UpgardeEnum, int> currentUpgradePoints = new Dictionary<UpgradeScriptableObject.UpgardeEnum, int>();

    public bool upgradeChancesInitialized = false;
    public bool currentChancesInitialized = false;
    public bool currentPointsInitialized = false;

    private string initialChanceFilePath;
    private string currentChanceFilePath;
    private string currentPointFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            initialChanceFilePath = Path.Combine(Application.persistentDataPath, "InitialUpgradeChanceData.json");
            currentChanceFilePath = Path.Combine(Application.persistentDataPath, "CurrentUpgradeChanceData.json");
            currentPointFilePath = Path.Combine(Application.persistentDataPath, "CurrentUpgradePointData.json");
            LoadCurrentChances();
            LoadCurrentPoints();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveCurrentChances()
    {
        UpgradeChanceDataList dataList = new UpgradeChanceDataList();
        foreach (var entry in currentUpgradeChances)
        {
            dataList.chances.Add(new UpgradeChanceData { upgradeType = entry.Key, chance = entry.Value });
        }

        string json = JsonUtility.ToJson(dataList, true);
        File.WriteAllText(currentChanceFilePath, json);
    }

    public void LoadCurrentChances()
    {
        if (File.Exists(currentChanceFilePath))
        {
            string json = File.ReadAllText(currentChanceFilePath);
            UpgradeChanceDataList dataList = JsonUtility.FromJson<UpgradeChanceDataList>(json);

            currentUpgradeChances.Clear();
            foreach (var data in dataList.chances)
            {
                currentUpgradeChances[data.upgradeType] = data.chance;
            }
            currentChancesInitialized = true;
        }
    }

    public void SaveCurrentPoints()
    {
        UpgradePointDataList dataList = new UpgradePointDataList();
        foreach (var entry in currentUpgradePoints)
        {
            dataList.points.Add(new UpgradePointData { upgradeType = entry.Key, points = entry.Value });
        }

        string json = JsonUtility.ToJson(dataList, true);
        File.WriteAllText(currentPointFilePath, json);
    }

    public void LoadCurrentPoints()
    {
        if (File.Exists(currentPointFilePath))
        {
            string json = File.ReadAllText(currentPointFilePath);
            UpgradePointDataList dataList = JsonUtility.FromJson<UpgradePointDataList>(json);

            currentUpgradePoints.Clear();
            foreach (var data in dataList.points)
            {
                currentUpgradePoints[data.upgradeType] = data.points;
            }
            currentPointsInitialized = true;
        }
    }
}