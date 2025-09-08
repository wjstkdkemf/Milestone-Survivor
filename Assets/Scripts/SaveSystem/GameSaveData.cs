[System.Serializable]
public class GameSaveData
{
    public InventoryData inventoryData;
    public PowerUpSaveData powerUpData;
    public PlayerStatsData playerStatsData;

    public GameSaveData(InventoryData invData, PowerUpSaveData powData, PlayerStatsData statsData)
    {
        inventoryData = invData;
        powerUpData = powData;
        playerStatsData = statsData;
    }
}
