using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SaveLoadManager : MonoBehaviour
{
    // Defines how the game should start in the next scene.
    
    public enum GameStartMode { None, NewGame, LoadGame , Running , ToIngame}
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
        if (!CanSaveGame())
        {
            return;
        }

        // Get save data from each manager.
        string invJson = InventorySystem.InventoryController.instance.GetSaveData();
        PowerUpSaveData powData = PowerUpManager.Instance.GetSaveData();
        PlayerStatsData statsData = PlayerStats.Instance.GetSaveData();

        // Create the main save data object.
        GameSaveData saveData = new GameSaveData(invJson, powData, statsData);
        SaveDataMigrator.PrepareForSave(saveData);
        Debug.Log("Loading Inventory JSON: " + saveData.inventoryJson);
        
        // Serialize to JSON and save to PlayerPrefs.
        string jsonData = JsonUtility.ToJson(saveData, true);
        string saveKey = SaveSlotKey + slotNumber;
        PlayerPrefs.SetString(saveKey, jsonData);
        PlayerPrefs.Save();

        GameProgressManager.Instance.SaveProgress();
        QuestManager.Instance.SaveQuestData();

        Debug.Log($"<color=green>[SaveLoadManager]</color> Game Saved to Slot {slotNumber}.");
    }

    public void LoadGame(int slotNumber)
    {
        if (!CanLoadGame())
        {
            return;
        }

        GameSaveData saveData = GetSaveSlotData(slotNumber);
        Debug.Log("체크");

        if (saveData != null)
        {
            // Distribute data to the managers.
            Debug.Log("Loading Inventory JSON: " + saveData.inventoryJson);
            InventorySystem.InventoryController.instance.LoadFromData(saveData.inventoryJson);
            // 불러온 장비 아이템 효과를 다시 적용합니다.
            InventorySystem.InventoryController.instance.ReapplyAllEquipmentEffects();
            InventorySystem.InventoryController.instance.ReapplyAllEquipmentSkills();
            InventorySystem.InventoryController.instance.SyncEquippedStatus("HotBar", "Inventory");
            EquipmentEffectManager.Instance.Change = false;

            PowerUpManager.Instance.LoadData(saveData.powerUpData);
            PlayerStats.Instance.LoadData(saveData.playerStatsData);
            QuestManager.Instance.LoadQuestData();
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
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearStoredInventory();
            InventoryManager.Instance.DeleteSavedInventoryFile("Current.json");
        }
        if (PowerUpManager.Instance != null) PowerUpManager.Instance.LoadData(null); // Will load defaults
        if (PlayerStats.Instance != null) PlayerStats.Instance.LoadData(null); // Will load defaults
        if (GameProgressManager.Instance != null) GameProgressManager.Instance.ResetProgress();
        if (QuestManager.Instance != null) QuestManager.Instance.ResetQuestDataForNewGame();
        if (TeleportManager.Instance != null) TeleportManager.Instance.ResetDataForNewGame();

        itemsToTransfer = null;
        IsLoadingFromFile = false; // Reset the flag
        Debug.Log("<color=yellow>[SaveLoadManager]</color> All data has been reset for a new game.");
    }

    private bool CanSaveGame()
    {
        bool canSave = true;

        if (InventorySystem.InventoryController.instance == null)
        {
            Debug.LogError("[SaveLoadManager] Cannot save: InventoryController is missing.");
            canSave = false;
        }
        if (PowerUpManager.Instance == null)
        {
            Debug.LogError("[SaveLoadManager] Cannot save: PowerUpManager is missing.");
            canSave = false;
        }
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("[SaveLoadManager] Cannot save: PlayerStats is missing.");
            canSave = false;
        }
        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("[SaveLoadManager] Cannot save: GameProgressManager is missing.");
            canSave = false;
        }
        if (QuestManager.Instance == null)
        {
            Debug.LogError("[SaveLoadManager] Cannot save: QuestManager is missing.");
            canSave = false;
        }

        return canSave;
    }

    private bool CanLoadGame()
    {
        bool canLoad = true;

        if (InventorySystem.InventoryController.instance == null)
        {
            Debug.LogError("[SaveLoadManager] Cannot load: InventoryController is missing.");
            canLoad = false;
        }
        if (PowerUpManager.Instance == null)
        {
            Debug.LogError("[SaveLoadManager] Cannot load: PowerUpManager is missing.");
            canLoad = false;
        }
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("[SaveLoadManager] Cannot load: PlayerStats is missing.");
            canLoad = false;
        }
        if (QuestManager.Instance == null)
        {
            Debug.LogError("[SaveLoadManager] Cannot load: QuestManager is missing.");
            canLoad = false;
        }

        return canLoad;
    }

    public GameSaveData GetSaveSlotData(int slotNumber)
    {
        string saveKey = SaveSlotKey + slotNumber;

        if (PlayerPrefs.HasKey(saveKey))
        {
            string jsonData = PlayerPrefs.GetString(saveKey);
            if (string.IsNullOrEmpty(jsonData)) return null;

            if (SaveDataMigrator.TryReadGameSave(jsonData, out GameSaveData saveData))
            {
                return saveData;
            }

            BackupInvalidSaveData(saveKey, jsonData);
            return null;
        }
        else
        {
            return null;
        }
    }

    private void BackupInvalidSaveData(string saveKey, string jsonData)
    {
        string backupKey = saveKey + "_InvalidBackup";
        PlayerPrefs.SetString(backupKey, jsonData);
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();
        Debug.LogWarning($"[SaveLoadManager] Invalid save data was moved to '{backupKey}'.");
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
