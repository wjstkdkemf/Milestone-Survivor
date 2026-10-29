using UnityEngine;
using System.Collections.Generic;
using System.IO;
using InventorySystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public int SceneName;

    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    private readonly HashSet<string> broughtEquipmentNames = new HashSet<string>(System.StringComparer.Ordinal);

    private bool isRestoring;
    
    //private InventoryData itemData;
    // If you need to store item prefabs, you can use another dictionary.
    // private Dictionary<string, GameObject> itemPrefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            DevLog.Log($"[InventoryManager] Awake: Instance created. GameObject: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[InventoryManager] Awake: Duplicate instance detected. Destroying new one. GameObject: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    public bool AddItem(string itemName, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemName) || quantity <= 0) return false;
        itemCounts.TryGetValue(itemName, out int current);
        if (quantity > int.MaxValue - current)
        {
            Debug.LogError($"[InventoryManager] Pending quantity overflow for '{itemName}'.");
            return false;
        }
        if (itemCounts.ContainsKey(itemName))
        {
            itemCounts[itemName] += quantity;
        }
        else
        {
            itemCounts[itemName] = quantity;
        }
        DevLog.Log($"Added {quantity} {itemName}. Total: {itemCounts[itemName]}");
        return true;
    }

    public Dictionary<string, int> GetInventoryData()
    {
        return new Dictionary<string, int>(itemCounts);
    }

    public void LoadInventoryData(Dictionary<string, int> data)
    {
        itemCounts.Clear();
        if (data != null)
            foreach (var item in data) AddItem(item.Key, item.Value);
        DevLog.Log("Inventory data loaded.");
    }

    public List<ItemSaveData> GetPendingSaveData()
    {
        var result = new List<ItemSaveData>(itemCounts.Count);
        foreach (var item in itemCounts)
            result.Add(new ItemSaveData(item.Value, item.Key, 0, 0));
        return result;
    }

    public void LoadPendingSaveData(List<ItemSaveData> data)
    {
        itemCounts.Clear();
        if (data == null) return;
        foreach (var item in data)
            if (item != null) AddItem(item.name, item.amount);
    }

    public void ClearInventory()
    {
        itemCounts.Clear();
        Debug.LogWarning("[InventoryManager] itemCounts cleared.");
        DevLog.Log(new System.Diagnostics.StackTrace().ToString());
    }

    public void ClearStoredInventory()
    {
        itemCounts.Clear();
        broughtEquipmentNames.Clear();
        
        DevLog.Log("[InventoryManager] Stored inventory cleared.");
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
        RestoreInventoryTo(InventoryController.ClearInventoryName);
    }

    public bool StoreInventoryFrom(string inventoryName)
    {
        DevLog.Log($"[InventoryManager] StoreInventoryFrom started. Current itemCounts: {itemCounts.Count}");
        if (InventoryController.instance == null)
        {
            Debug.LogError("InventoryController not found.");
            return false;
        }

        Inventory inventory = InventoryController.instance.GetInventory(inventoryName);
        if (inventory == null)
        {
            Debug.LogError($"Inventory '{inventoryName}' not found.");
            return false;
        }

        //ClearInventory(); // Clear previous data before storing new data.

        bool fullyStored = true;

        // 던전 전리품을 정산할 때만 반입 장비를 제외
        bool isDungeonSettlement = inventoryName == InventoryController.ClearInventoryName;

        List<InventoryItem> items = new List<InventoryItem>(inventory.GetList());

        foreach (InventoryItem item in items)
        {
            if (item == null || item.GetIsNull())
                continue;

            string itemName = item.GetItemType();
            int amount = item.GetAmount();

            if (isDungeonSettlement && IsBroughtEquipment(itemName))
            {
                // 마을에 원본이 있으므로 던전의 사본만 제거
                inventory.RemoveItemInPosition(item.GetPosition(), amount);
                continue;
            }

            // 실제 획득품은 보관 성공 후 원래 슬롯에서 제거
            if (AddItem(itemName, amount))
            {
                inventory.RemoveItemInPosition(item.GetPosition(), amount);
            }
            else
            {
                fullyStored = false;
            }
        }

        // 일부 보관에 실패했다면 재시도를 위해 기록 유지
        if (isDungeonSettlement && fullyStored)
        {
            broughtEquipmentNames.Clear();
        }

        return fullyStored;
    }

    public void RestoreInventoryTo(string inventoryName)
    {
        if (isRestoring || itemCounts.Count == 0) return;
        DevLog.Log($"[InventoryManager] RestoreInventoryTo started. Current itemCounts: {itemCounts.Count}");
        if (InventoryController.instance == null)
        {
            Debug.LogError("InventoryController not found.");
            return;
        }

        isRestoring = true;
        try
        {
            // Only retire quantities that were actually added or converted.
            foreach (var item in new Dictionary<string, int>(itemCounts))
            {
                ItemAddResult result = InventoryController.instance.AddItemWithResult(inventoryName, item.Key, item.Value);
                int processed = result.Added + result.Converted;
                if (processed == 0) continue;
                itemCounts[item.Key] -= processed;
                if (itemCounts[item.Key] == 0) itemCounts.Remove(item.Key);
            }
        }
        finally
        {
            isRestoring = false;
        }
        DevLog.Log($"[InventoryManager] Restore completed. Pending item types: {itemCounts.Count}.");
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

        DevLog.Log($"모든 인벤토리가 {GetFullPath(fileName)} 파일에 저장되었습니다.");
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

            DevLog.Log($"{path} 파일에서 모든 인벤토리를 불러왔습니다.");
        }
        else
        {
            Debug.LogWarning($"저장 파일을 찾을 수 없습니다: {path}. 인벤토리를 비웁니다.");
            // 저장 파일이 없으면 모든 인벤토리를 깨끗하게 비웁니다.
            InventoryController.instance.ClearAllInventories();
        }
    }
    public void DeleteSavedInventoryFile(string fileName)
    {
        string path = GetFullPath(fileName);
        if (!File.Exists(path)) return;

        File.Delete(path);
        DevLog.Log($"[InventoryManager] Deleted saved inventory file: {path}");
    }
    // 새 던전 진입 시 호출
    public void BeginDungeonInventoryTracking()
    {
        broughtEquipmentNames.Clear();
    }

    // 실제 복사에 성공한 장비만 등록
    public void TrackBroughtEquipment(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            return;

        broughtEquipmentNames.Add(itemName);
    }

    // 복귀 정산에서 반입 장비인지 확인
    public bool IsBroughtEquipment(string itemName)
    {
        return !string.IsNullOrEmpty(itemName) &&
            broughtEquipmentNames.Contains(itemName);
    }
}
