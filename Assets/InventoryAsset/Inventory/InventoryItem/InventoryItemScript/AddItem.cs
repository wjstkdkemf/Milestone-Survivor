using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    //Author: Jaxon Schauer
    /// <summary>
    /// Holds information given from the drop down menu 
    /// </summary>
    public class AddItem : MonoBehaviour
    {
        [SerializeField]
        private int amount;
        [HideInInspector]
        private List<ItemData> items;

        [HideInInspector]
        private List<InventoryInitializer> inventories;

        [HideInInspector]
        public int selectedItemIndex = 0;

        [HideInInspector]
        public int SelectedInventoryIndex = 0;
        [SerializeField, HideInInspector]
        private ItemData item;
        [SerializeField, HideInInspector]
        private InventoryInitializer inventory;
        [SerializeField, HideInInspector]
        GameObject controller;

        public void FindController()
        {
            controller = GameObject.Find("InventoryController");

        }
        public void FindItemList()
        {
            if (controller == null)
            {
                FindController();
            }
            items = controller.GetComponent<InventoryController>().items;
        }
        public void FindInventoryList()
        {
            if (controller == null)
            {
                FindController();
            }
            inventories = controller.GetComponent<InventoryController>().initializeInventory;
        }
        private void Awake()
        {
            gameObject.SetActive(false);
        }
        private void OnEnable()
        {
            if (InventoryController.instance == null)
            {
                Destroy(gameObject);
            }
            if (amount != 0)
            {
                InventoryController.instance.AddItem(inventory.GetInventoryName(), item.itemName, amount);
            }
            else
            {
                InventoryController.instance.AddItem(inventory.GetInventoryName(), item.itemName);
            }
            gameObject.SetActive(false);
        }
        public void SetItem(ItemData data)
        {
            item = data;
        }
        public void SetInventory(InventoryInitializer init)
        {
            inventory = init;
        }
        public void SetInventoryList(List<InventoryInitializer> init)
        {
            inventories = init;
        }
        public List<InventoryInitializer> GetInvList()
        {
            return inventories;
        }
        public void SetItemList(List<ItemData> data)
        {
            items = data;
        }
        public List<ItemData> GetItemsList()
        {
            return items;
        }
    }

}

