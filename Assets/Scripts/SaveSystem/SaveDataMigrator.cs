using UnityEngine;

public static class SaveDataMigrator
{
    public const int CurrentGameSaveVersion = 1;

    public static bool TryReadGameSave(string jsonData, out GameSaveData saveData)
    {
        saveData = null;

        if (string.IsNullOrEmpty(jsonData) || jsonData.Trim().Length == 0)
        {
            Debug.LogWarning("[SaveDataMigrator] Save data is empty.");
            return false;
        }

        try
        {
            saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[SaveDataMigrator] Failed to parse save data: {exception.Message}");
            return false;
        }

        if (saveData == null)
        {
            Debug.LogError("[SaveDataMigrator] Save data parsed to null.");
            return false;
        }

        MigrateGameSave(saveData);
        return true;
    }

    public static void PrepareForSave(GameSaveData saveData)
    {
        if (saveData == null) return;

        saveData.saveVersion = CurrentGameSaveVersion;
        saveData.appVersion = Application.version;
    }

    private static void MigrateGameSave(GameSaveData saveData)
    {
        int originalVersion = saveData.saveVersion;

        if (saveData.saveVersion <= 0)
        {
            MigrateLegacyGameSave(saveData);
        }

        RepairGameSave(saveData);

        if (saveData.saveVersion > CurrentGameSaveVersion)
        {
            Debug.LogWarning($"[SaveDataMigrator] Save version {saveData.saveVersion} is newer than supported version {CurrentGameSaveVersion}. Loading with best effort.");
        }

        if (originalVersion != saveData.saveVersion)
        {
            Debug.Log($"[SaveDataMigrator] Migrated save data from version {originalVersion} to {saveData.saveVersion}.");
        }
    }

    private static void MigrateLegacyGameSave(GameSaveData saveData)
    {
        if (saveData.inventoryJson == null) saveData.inventoryJson = "";
        saveData.saveVersion = 1;
        saveData.appVersion = string.IsNullOrEmpty(saveData.appVersion) ? "Legacy" : saveData.appVersion;
    }

    private static void RepairGameSave(GameSaveData saveData)
    {
        if (saveData.inventoryJson == null) saveData.inventoryJson = "";
        if (saveData.playerStatsData == null) saveData.playerStatsData = new PlayerStatsData();

        if (saveData.playerStatsData.level <= 0) saveData.playerStatsData.level = 1;
        if (saveData.playerStatsData.requiredXP <= 0) saveData.playerStatsData.requiredXP = 50;
        if (saveData.playerStatsData.currentXP < 0) saveData.playerStatsData.currentXP = 0;
        if (saveData.playerStatsData.goldAmount < 0) saveData.playerStatsData.goldAmount = 0;
        if (saveData.playerStatsData.stageCleared < 0) saveData.playerStatsData.stageCleared = 0;
        if (saveData.playerStatsData.characterID < 0) saveData.playerStatsData.characterID = 0;
    }
}
