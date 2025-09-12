using System;
using UnityEngine;

namespace InventorySystem
{
    //Author: Jaxon Schauer
    /// <summary>
    /// Contains information about an item to be passed through code
    /// </summary>

    public class InventoryItem
    {
        private int amount;

        string itemType;
        private Sprite itemImage;//Holds image of item
        private int maxStackAmount;
        private bool draggable;
        private bool pressable;
        private InventoryItemEvent itemEvent;
        private GameObject relatedGameObject;
        private bool isNull = false;//Checks if item exists
        private bool displayAmount;
        private int position;
        private string inventory;
        private string previousInventory;
        public InventoryItem(ItemData data)
        {
            this.amount = 1;
            this.itemType = data.itemName;
            this.itemImage = data.icon;
            this.maxStackAmount = data.maxStackAmount;
            this.draggable = data.draggable;
            this.pressable = data.pressable;
            this.itemEvent = data.itemAction;
            this.isNull = (data == null);
            this.relatedGameObject = data.RelatedGameObject;
            this.displayAmount = data.displayItemAmount;
        }
        public InventoryItem(InventoryItem other, int amount = 1)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Passed argument is null");
            }

            this.amount = amount;
            this.itemType = other.itemType != null ? string.Copy(other.itemType) : null;
            this.itemImage = other.itemImage;

            this.maxStackAmount = other.maxStackAmount;
            this.draggable = other.draggable;
            this.pressable = other.pressable;
            this.itemEvent = other.itemEvent;
            this.isNull = other.isNull;
            this.relatedGameObject = other.GetRelatedGameObject();
            this.displayAmount = other.GetDisplayAmount();
            this.inventory = other.inventory;
            this.position = other.position;
            this.previousInventory= other.previousInventory;
        }

        public InventoryItem(bool isNull)
        {
            amount = 1;
            this.isNull = isNull;
        }
        public void SetIsNull(bool isNull)
        {
            this.isNull = isNull;
        }
        public bool GetIsNull()
        {
            return isNull;
        }
        public string GetItemType()
        {
            return itemType;
        }
        public Sprite GetItemImage()
        {
            return itemImage;
        }
        public int GetItemStackAmount()
        {
            return maxStackAmount;
        }
        public void Selected()
        {
            if (itemEvent != null)
                itemEvent.Invoke(this);
        }
        public bool GetPressable()
        {
            return pressable;
        }
        public bool GetDraggable()
        {
            return draggable;
        }
        public int GetAmount()
        {
            return amount;
        }
        public void SetAmount(int amount)
        {
            this.amount = amount;
        }
        public bool GetDisplayAmount()
        {
            return displayAmount;
        }
        public GameObject GetRelatedGameObject()
        {
            return relatedGameObject;
        }
        public void SetPressable(bool pressable)
        {
            this.pressable = pressable;
        }
        public void SetPosition(int position)
        {
            this.position = position;
        }
        public int GetPosition()
        {
            return position;
        }
        public void SetInventory(string inventory)
        {
            previousInventory = this.inventory;
            this.inventory = inventory;
        }
        public string GetInventory()
        {
            return inventory;
        }
        public override string ToString()
        {
            string result = $@"
            ItemType: {itemType}
            Inventory: {inventory}
            previousInventory: {previousInventory}
            Item Position: {position}
            Item Amount: {amount}
            Max Item Amount: {maxStackAmount}";
            return result;



        }
    }
}
