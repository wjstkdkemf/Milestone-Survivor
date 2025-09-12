using UnityEngine;
using System.Collections.Generic;
using InventorySystem;

/// <summary>
/// Finds the InventoryController and manually registers a list of UI GameObjects.
/// This script must have its execution order set to run before the default time and before InventoryController.
/// </summary>
public class ManualInventoryRegistrar : MonoBehaviour
{
    public List<GameObject> inventoryUIs;

    void Awake()
    {
        InventoryController controller = InventoryController.instance;

        if (controller != null)
        {
            // Clear any persistent "ghost" references from the editor
            controller.ClearRegisteredUI();

            if (inventoryUIs != null && inventoryUIs.Count > 0)
            {
                foreach (GameObject ui in inventoryUIs)
                {
                    controller.RegisterExternalUI(ui);
                }
                Debug.Log("ManualInventoryRegistrar: Cleared and re-registered " + inventoryUIs.Count + " external inventories.");
            }
        }
        else
        {
            Debug.LogError("ManualInventoryRegistrar: Could not find InventoryController instance in the scene!");
        }
    }
}
