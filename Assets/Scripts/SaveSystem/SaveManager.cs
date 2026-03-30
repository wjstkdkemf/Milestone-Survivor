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
        Debug.Log("Game Saved: " + savePath);
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
            Debug.Log("Save file deleted.");
        }
    }
}
[System.Serializable]
public class PowerUpSaveData
{
    public List<PowerUpType> powerUpTypes = new List<PowerUpType>();
    public List<int> powerUpLevels = new List<int>();

    public PowerUpSaveData(Dictionary<PowerUpType, int> levels)
    {
        foreach (var pair in levels)
        {
            powerUpTypes.Add(pair.Key);
            powerUpLevels.Add(pair.Value);
        }
    }

    public Dictionary<PowerUpType, int> ToDictionary()
    {
        var dict = new Dictionary<PowerUpType, int>();
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