using UnityEngine;
using System.Collections.Generic;

namespace InventorySystem
{
    // Author: Jaxon Schauer
    // Modified by Gemini to use PlayerPrefs and JsonUtility

    public static class InventorySaveSystem
    {
        // Saves the inventory data to PlayerPrefs as a JSON string.
        public static void SaveInventory(Dictionary<string, Inventory> inventoryManager, string saveKey)
        {
            InventoryData inventoryData = new InventoryData(inventoryManager);
            string jsonData = JsonUtility.ToJson(inventoryData, true);
            PlayerPrefs.SetString(saveKey, jsonData);
            Debug.Log($"<color=green>[InventorySaveSystem]</color> Inventory saved to PlayerPrefs with key: {saveKey}");
        }

        // Loads the inventory data from PlayerPrefs.
        public static InventoryData LoadItem(string saveKey)
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                string jsonData = PlayerPrefs.GetString(saveKey);
                if (string.IsNullOrEmpty(jsonData))
                {
                    Debug.LogWarning($"<color=orange>[InventorySaveSystem]</color> No data found for key: {saveKey}. Returning null.");
                    return null;
                }

                InventoryData inventoryData = JsonUtility.FromJson<InventoryData>(jsonData);
                Debug.Log($"<color=green>[InventorySaveSystem]</color> Inventory loaded from PlayerPrefs with key: {saveKey}");
                return inventoryData;
            }
            else
            {
                Debug.LogWarning($"<color=orange>[InventorySaveSystem]</color> Save key not found: {saveKey}. Returning null.");
                return null;
            }
        }

        // Deletes a specific inventory save from PlayerPrefs.
        public static void Reset(string saveKey)
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                PlayerPrefs.DeleteKey(saveKey);
                Debug.Log($"<color=yellow>[InventorySaveSystem]</color> Inventory save deleted for key: {saveKey}");
            }
        }
    }
}