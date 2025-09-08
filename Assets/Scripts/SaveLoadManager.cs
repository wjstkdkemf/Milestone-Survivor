using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    public bool IsLoadingFromFile { get; private set; } = false;

    private const string SaveSlotKey = "SaveSlot_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SaveGame(int slotNumber)
    {
        if (InventoryManager.Instance == null || PowerUpManager.Instance == null || PlayerStats.Instance == null)
        {
            Debug.LogError("A manager is missing. Cannot save game.");
            return;
        }

        InventoryData invData = new InventoryData(InventoryManager.Instance.GetInventoryData());
        PowerUpSaveData powData = PowerUpManager.Instance.GetSaveData();
        PlayerStatsData statsData = PlayerStats.Instance.GetSaveData();

        GameSaveData saveData = new GameSaveData(invData, powData, statsData);

        string jsonData = JsonUtility.ToJson(saveData, true);
        string saveKey = SaveSlotKey + slotNumber;
        PlayerPrefs.SetString(saveKey, jsonData);
        PlayerPrefs.Save();

        Debug.Log($"Game Saved to Slot {slotNumber}.");
    }

    public void LoadGame(int slotNumber)
    {
        GameSaveData saveData = GetSaveSlotData(slotNumber);

        if (saveData != null)
        {
            // Distribute data to the managers
            InventoryManager.Instance.LoadInventoryData(saveData.inventoryData.ToDictionary());
            PowerUpManager.Instance.LoadData(saveData.powerUpData);
            PlayerStats.Instance.LoadData(saveData.playerStatsData);
            IsLoadingFromFile = true; // Set the flag
            Debug.Log($"Game Loaded from Slot {slotNumber}.");
        }
        else
        {
            // No save data found, prepare for a new game
            Debug.LogWarning($"No save data found for Slot {slotNumber}. Preparing for new game.");
            ClearAllDataForNewGame();
        }
    }

    public void ClearAllDataForNewGame()
    {
        if (InventoryManager.Instance != null) InventoryManager.Instance.ClearInventory();
        if (PowerUpManager.Instance != null) PowerUpManager.Instance.LoadData(null); // Will load defaults
        if (PlayerStats.Instance != null) PlayerStats.Instance.LoadData(null); // Will load defaults
        IsLoadingFromFile = false; // Reset the flag
        Debug.Log("All data has been reset for a new game.");
    }

    public GameSaveData GetSaveSlotData(int slotNumber)
    {
        string saveKey = SaveSlotKey + slotNumber;

        if (PlayerPrefs.HasKey(saveKey))
        {
            string jsonData = PlayerPrefs.GetString(saveKey);
            return JsonUtility.FromJson<GameSaveData>(jsonData);
        }
        else
        {
            return null;
        }
    }

    public void DeleteSaveData(int slotNumber)
    {
        string saveKey = SaveSlotKey + slotNumber;
        if (PlayerPrefs.HasKey(saveKey))
        {
            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.Save();
            Debug.Log($"Save data for slot {slotNumber} deleted.");
        }
    }
}