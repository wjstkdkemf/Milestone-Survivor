using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string savePath => Path.Combine(Application.persistentDataPath, "powerUpSave.json");
    private static string UpgradesavePath => Path.Combine(Application.persistentDataPath, "UpGradeSave.json");

    public static void Save(PowerUpSaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        DevLog.Log("Game Saved: " + savePath);
    }

    public static PowerUpSaveData Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found.");
            return null;
        }

        string json = File.ReadAllText(savePath);
        return JsonUtility.FromJson<PowerUpSaveData>(json);
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            DevLog.Log("Save file deleted.");
        }
    }
}
[System.Serializable]
public class PowerUpSaveData
{
    public List<string> powerUpTypes = new List<string>();
    public List<int> powerUpLevels = new List<int>();

    public PowerUpSaveData(Dictionary<string, int> levels)
    {
        foreach (var pair in levels)
        {
            powerUpTypes.Add(pair.Key);
            powerUpLevels.Add(pair.Value);
        }
    }

    public Dictionary<string, int> ToDictionary()
    {
        var dict = new Dictionary<string, int>();
        for (int i = 0; i < powerUpTypes.Count; i++)
        {
            if (i < powerUpLevels.Count)
            {
                dict[powerUpTypes[i]] = powerUpLevels[i];
            }
        }
        return dict;
    }
}
