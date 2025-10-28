using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    // Author: Jaxon Schauer
    // Modified by Gemini to be compatible with JsonUtility

    [System.Serializable]
    public class SerializableInventory
    {
        public string inventoryName;
        public List<ItemSaveData> items;

        public SerializableInventory(string name, List<ItemSaveData> itemData)
        {
            inventoryName = name;
            items = itemData;
        }
    }

    [System.Serializable]
    public class InventoryData
    {
        public List<SerializableInventory> inventories;

        // Constructor for saving
        public InventoryData(Dictionary<string, Inventory> inventoryManager)
        {
            inventories = new List<SerializableInventory>();
            foreach (var pair in inventoryManager)
            {
                if (!inventoryManager[pair.Key].GetSaveInventory())
                {
                    continue;
                }

                List<ItemSaveData> itemData = new List<ItemSaveData>();
                Inventory inventory = pair.Value;
                int position = 0;
                foreach(InventoryItem item in inventory.GetList())
                {
                    // null이나 "Empty" 타입 아이템은 저장하지 않음
                    if (item != null && !item.GetIsNull() && !string.IsNullOrEmpty(item.GetItemType()))
                    {
                        itemData.Add(new ItemSaveData(item.GetAmount(), item.GetItemType(), position, item.GetEnhancementLevel()));
                    }
                    position++;
                }

                // 아이템이 있는 인벤토리만 저장
                if (itemData.Count > 0)
                {
                    inventories.Add(new SerializableInventory(inventory.GetName(), itemData));
                }
            }
        }

        // Helper method for loading
        public Dictionary<string, List<ItemSaveData>> ToDictionary()
        {
            var dict = new Dictionary<string, List<ItemSaveData>>();
            foreach (var serializableInventory in inventories)
            {
                dict[serializableInventory.inventoryName] = serializableInventory.items;
            }
            return dict;
        }
    }
}