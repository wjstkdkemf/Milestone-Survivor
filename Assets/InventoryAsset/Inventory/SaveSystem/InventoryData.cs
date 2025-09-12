using System.Collections.Generic;
namespace InventorySystem
{
    //Author Jaxon Schauer
    /// <summary>
    /// creates a dictionary that holds all information related to the inventories.
    /// </summary>
    [System.Serializable]
    public class InventoryData
    {
        public Dictionary<string, List<ItemSaveData>> inventories;

        public string itemType;
        public InventoryData(Dictionary<string, Inventory> inventoryManager)
        {
            inventories = new Dictionary<string, List<ItemSaveData>>();
            foreach (var pair in inventoryManager)
            {
                int position = 0;
                if (!inventoryManager[pair.Key].GetSaveInventory())
                {
                    continue;
                }
                List<ItemSaveData> itemData = new List<ItemSaveData>();
                Inventory inventory = pair.Value;
                foreach(InventoryItem item in inventory.GetList())
                {
                    itemData.Add(new ItemSaveData(item.GetAmount(),item.GetItemType(), position));
                    position++;
                }
                inventories.Add(inventory.GetName(), itemData);
            }
        }
    }
}
