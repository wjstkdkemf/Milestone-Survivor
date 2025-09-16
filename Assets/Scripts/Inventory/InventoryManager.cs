using UnityEngine;
using System.Collections.Generic;
using InventorySystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public int SceneName;

    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    // If you need to store item prefabs, you can use another dictionary.
    // private Dictionary<string, GameObject> itemPrefabs = new Dictionary<string, GameObject>();

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
        Debug.Log("Inventory cleared.");
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
}
