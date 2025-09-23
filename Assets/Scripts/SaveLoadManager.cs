using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SaveLoadManager : MonoBehaviour
{
    // Defines how the game should start in the next scene.
    
    public enum GameStartMode { None, NewGame, LoadGame , Running}
    // Stores the choice from the main menu.
    public GameStartMode startMode { get; set; } = GameStartMode.None;

    public static SaveLoadManager Instance { get; private set; }

    public bool IsLoadingFromFile { get; private set; } = false;
    public int selectedIndex { get; set; }

    // Temporary storage for transferring items between scenes.
    public List<InventorySystem.ItemSaveData> itemsToTransfer = null;

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
        // Check for the essential managers. InventoryManager is no longer needed.
        if (InventorySystem.InventoryController.instance == null || PowerUpManager.Instance == null || PlayerStats.Instance == null)
        {
            Debug.LogError("A manager is missing. Cannot save game.");
            return;
        }

        // Get save data from each manager.
        string invJson = InventorySystem.InventoryController.instance.GetSaveData();
        PowerUpSaveData powData = PowerUpManager.Instance.GetSaveData();
        PlayerStatsData statsData = PlayerStats.Instance.GetSaveData();

        // Create the main save data object.
        GameSaveData saveData = new GameSaveData(invJson, powData, statsData);
        Debug.Log("Loading Inventory JSON: " + saveData.inventoryJson);
        
        // Serialize to JSON and save to PlayerPrefs.
        string jsonData = JsonUtility.ToJson(saveData, true);
        string saveKey = SaveSlotKey + slotNumber;
        PlayerPrefs.SetString(saveKey, jsonData);
        PlayerPrefs.Save();

        Debug.Log($"<color=green>[SaveLoadManager]</color> Game Saved to Slot {slotNumber}.");
    }

    public void LoadGame(int slotNumber)
    {
        GameSaveData saveData = GetSaveSlotData(slotNumber);

        if (saveData != null)
        {
            // Distribute data to the managers.
            Debug.Log("Loading Inventory JSON: " + saveData.inventoryJson);
            InventorySystem.InventoryController.instance.LoadFromData(saveData.inventoryJson);
            PowerUpManager.Instance.LoadData(saveData.powerUpData);
            PlayerStats.Instance.LoadData(saveData.playerStatsData);
            IsLoadingFromFile = true; // Set the flag
            Debug.Log($"<color=green>[SaveLoadManager]</color> Game Loaded from Slot {slotNumber}.");
        }
        else
        {
            // No save data found, prepare for a new game.
            Debug.LogWarning($"<color=orange>[SaveLoadManager]</color> No save data found for Slot {slotNumber}. Preparing for new game.");
            ClearAllDataForNewGame();
        }
    }

    public void ClearAllDataForNewGame()
    {
        // Clear data in all managers for a fresh start.
        if (InventorySystem.InventoryController.instance != null) InventorySystem.InventoryController.instance.ClearAllInventories();
        if (PowerUpManager.Instance != null) PowerUpManager.Instance.LoadData(null); // Will load defaults
        if (PlayerStats.Instance != null) PlayerStats.Instance.LoadData(null); // Will load defaults
        IsLoadingFromFile = false; // Reset the flag
        Debug.Log("<color=yellow>[SaveLoadManager]</color> All data has been reset for a new game.");
    }

    public GameSaveData GetSaveSlotData(int slotNumber)
    {
        string saveKey = SaveSlotKey + slotNumber;

        if (PlayerPrefs.HasKey(saveKey))
        {
            string jsonData = PlayerPrefs.GetString(saveKey);
            if (string.IsNullOrEmpty(jsonData)) return null;
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
            Debug.Log($"<color=yellow>[SaveLoadManager]</color> Save data for slot {slotNumber} deleted.");
        }
    }

    public void SettingMode(int mode)
    {
        startMode = (GameStartMode)mode;
        Debug.Log(startMode);
    }
}