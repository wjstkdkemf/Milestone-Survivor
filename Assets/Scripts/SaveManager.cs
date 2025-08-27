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
    public Dictionary<PowerUpType, int> powerUpLevels;
    public int playerGold;

    public PowerUpSaveData(Dictionary<PowerUpType, int> levels, int gold)
    {
        powerUpLevels = levels;
        playerGold = gold;
    }
}