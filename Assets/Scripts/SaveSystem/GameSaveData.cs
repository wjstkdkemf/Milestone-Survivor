[System.Serializable]
public class GameSaveData
{
    public int saveVersion;
    public string appVersion;

    // This now stores the inventory as a JSON string, not a direct object.
    public string inventoryJson;
    public PowerUpSaveData powerUpData;
    public PlayerStatsData playerStatsData;
    public System.Collections.Generic.List<InventorySystem.ItemSaveData> pendingInventoryItems
        = new System.Collections.Generic.List<InventorySystem.ItemSaveData>();

    public GameSaveData(string invJson, PowerUpSaveData powData, PlayerStatsData statsData)
    {
        saveVersion = SaveDataMigrator.CurrentGameSaveVersion;
        appVersion = UnityEngine.Application.version;
        inventoryJson = invJson;
        powerUpData = powData;
        playerStatsData = statsData;
    }
}
