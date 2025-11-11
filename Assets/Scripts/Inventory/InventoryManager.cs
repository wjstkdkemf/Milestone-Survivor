using UnityEngine;
using System.Collections.Generic;
using System.IO;
using InventorySystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public int SceneName;

    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    private InventoryData itemData;
    // If you need to store item prefabs, you can use another dictionary.
    // private Dictionary<string, GameObject> itemPrefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[InventoryManager] Awake: Instance created. GameObject: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[InventoryManager] Awake: Duplicate instance detected. Destroying new one. GameObject: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    public void AddItem(string itemName, int quantity = 1)
    {
        if (itemCounts.ContainsKey(itemName))
        {
            itemCounts[itemName] += quantity;
        }
        else
        {
            itemCounts[itemName] = quantity;
        }
        Debug.Log($"Added {quantity} {itemName}. Total: {itemCounts[itemName]}");
    }

    public Dictionary<string, int> GetInventoryData()
    {
        return new Dictionary<string, int>(itemCounts);
    }

    public void LoadInventoryData(Dictionary<string, int> data)
    {
        itemCounts = new Dictionary<string, int>(data);
        Debug.Log("Inventory data loaded.");
    }

    public void ClearInventory()
    {
        itemCounts.Clear();
        Debug.LogWarning("[InventoryManager] itemCounts cleared.");
        Debug.Log(new System.Diagnostics.StackTrace().ToString());
    }
    public void InsertGame(int gameName = 100)
    {
        if (gameName == 100)
            SceneName = LoadScreenManager.Instance.currentlySelectedSlot.slotId;
        else
            SceneName = gameName;
    }
    public void ClearMapInventory()
    {
        foreach (var item in itemCounts)
        {
            InventoryController.instance.AddItem("ClearInventory", item.Key, item.Value);
        }
    }

    public void StoreInventoryFrom(string inventoryName)
    {
        Debug.Log($"[InventoryManager] StoreInventoryFrom started. Current itemCounts: {itemCounts.Count}");
        if (InventoryController.instance == null)
        {
            Debug.LogError("InventoryController not found.");
            return;
        }

        Inventory inventory = InventoryController.instance.GetInventory(inventoryName);
        if (inventory == null)
        {
            Debug.LogError($"Inventory '{inventoryName}' not found.");
            return;
        }

        //ClearInventory(); // Clear previous data before storing new data.

        List<InventoryItem> items = inventory.GetList();
        foreach (InventoryItem item in items)
        {
            if (item != null && !item.GetIsNull())
            {
                AddItem(item.GetItemType(), item.GetAmount());
            }
        }
        Debug.Log($"[InventoryManager] Stored {itemCounts.Count} item types from {inventoryName}.");
    }

    public void RestoreInventoryTo(string inventoryName)
    {
        Debug.Log($"[InventoryManager] RestoreInventoryTo started. Current itemCounts: {itemCounts.Count}");
        if (InventoryController.instance == null)
        {
            Debug.LogError("InventoryController not found.");
            return;
        }

        foreach (var item in itemCounts)
        {
            InventoryController.instance.AddItem(inventoryName, item.Key, item.Value);
        }
        Debug.Log($"[InventoryManager] Restored {itemCounts.Count} item types to {inventoryName}.");

        ClearInventory(); // Clear after restoring.
    }
    private string GetFullPath(string fileName)
      {
          return Path.Combine(Application.persistentDataPath, fileName);
      }
    public void SaveAllInventories(string fileName)
    {
        if (InventoryController.instance == null)
        {
            Debug.LogError("저장 실패: InventoryController 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // InventoryController에서 모든 인벤토리 데이터를 JSON 문자열로 가져옵니다.
        string jsonData = InventoryController.instance.GetSaveData();

        // JSON 문자열을 파일에 씁니다.
        File.WriteAllText(GetFullPath(fileName), jsonData);

        Debug.Log($"모든 인벤토리가 {GetFullPath(fileName)} 파일에 저장되었습니다.");
    }
    public void LoadAllInventories(string fileName)
    {
        if (InventoryController.instance == null)
        {
            Debug.LogError("로드 실패: InventoryController 인스턴스를 찾을 수 없습니다.");
            return;
        }

        string path = GetFullPath(fileName);

        if (File.Exists(path))
        {
            // 파일에서 JSON 문자열을 읽어옵니다.
            string jsonData = File.ReadAllText(path);

            // InventoryController를 사용해 JSON 데이터로부터 인벤토리를 복원합니다.
            InventoryController.instance.LoadFromData(jsonData);
            //InventoryController.instance.CopyEquippedItemsToInventory("HotBar", "ClearInventory");

            Debug.Log($"{path} 파일에서 모든 인벤토리를 불러왔습니다.");
        }
        else
        {
            Debug.LogWarning($"저장 파일을 찾을 수 없습니다: {path}. 인벤토리를 비웁니다.");
            // 저장 파일이 없으면 모든 인벤토리를 깨끗하게 비웁니다.
            InventoryController.instance.ClearAllInventories();
        }
    }
}
