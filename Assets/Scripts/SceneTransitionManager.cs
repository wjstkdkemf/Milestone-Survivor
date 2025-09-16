using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using InventorySystem;

/// <summary>
/// Example script demonstrating how to transfer inventory items between scenes
/// using the SaveLoadManager as a temporary data carrier.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    /// <summary>
    /// Call this method BEFORE loading the village scene.
    /// It collects all items from a specified inventory and stores them for transfer.
    /// </summary>
    /// <param name="fromInventoryName">The name of the inventory to collect items from in the current scene.</param>
    public static void PrepareForVillage(string fromInventoryName)
    {
        if (InventoryController.instance == null)
        {
            Debug.LogError("[SceneTransitionManager] InventoryController not found in the current scene. Cannot prepare items for transfer.");
            return;
        }

        Inventory sourceInventory = InventoryController.instance.GetInventory(fromInventoryName);
        if (sourceInventory == null)
        {
            Debug.LogError($"[SceneTransitionManager] Source inventory '{fromInventoryName}' not found.");
            return;
        }

        List<ItemSaveData> items = new List<ItemSaveData>();
        foreach (var item in sourceInventory.GetList())
        {
            if (item != null && !item.GetIsNull())
            {
                // Position is not important for transferring, so we can use 0.
                items.Add(new ItemSaveData(item.GetAmount(), item.GetItemType(), 0));
            }
        }

        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.itemsToTransfer = items;
            Debug.Log($"<color=cyan>[SceneTransitionManager]</color> {items.Count} item stacks prepared for transfer from '{fromInventoryName}'.");
        }
        else
        {
            Debug.LogError("[SceneTransitionManager] SaveLoadManager not found. Cannot store items for transfer.");
        }
    }

    /// <summary>
    /// Call this method in the Start() of a manager script in the village scene.
    /// It checks for transferred items and adds them to the specified inventory.
    /// </summary>
    /// <param name="toInventoryName">The name of the inventory to add items to in the village scene.</param>
    public static void CheckForTransferredItems(string toInventoryName)
    {
        if (SaveLoadManager.Instance == null || SaveLoadManager.Instance.itemsToTransfer == null || SaveLoadManager.Instance.itemsToTransfer.Count == 0)
        {
            // No items to transfer, do nothing.
            return;
        }

        if (InventoryController.instance == null)
        {
            Debug.LogError("[SceneTransitionManager] InventoryController not found in the village scene. Cannot add transferred items.");
            // Clear the list anyway to prevent issues
            SaveLoadManager.Instance.itemsToTransfer = null;
            return;
        }

        Debug.Log($"<color=cyan>[SceneTransitionManager]</color> Found {SaveLoadManager.Instance.itemsToTransfer.Count} item stacks to transfer to '{toInventoryName}'.");

        foreach (var itemData in SaveLoadManager.Instance.itemsToTransfer)
        {
            InventoryController.instance.AddItem(toInventoryName, itemData.name, itemData.amount);
        }

        // Important: Clear the list after transferring to prevent re-adding them later.
        SaveLoadManager.Instance.itemsToTransfer = null;
    }

    // --- EXAMPLE USAGE --- //

    /*
    // In your Gameplay Scene, when you want to go to the village:
    public void GoToVillage()
    {
        // 1. Prepare items from the 'PlayerInventory' for transfer.
        SceneTransitionManager.PrepareForVillage("PlayerInventory");

        // 2. Load the village scene.
        SceneManager.LoadScene("VillageSceneName"); // <-- Replace with your actual village scene name
    }
    */

    /*
    // In a script in your Village Scene (e.g., VillageManager.cs):
    void Start()
    {
        // 1. Check for and add any items that were transferred from the gameplay scene.
        SceneTransitionManager.CheckForTransferredItems("Storage"); // <-- Assumes you have an inventory named "Storage"
    }
    */
}