using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace InventorySystem
{
    //Author Jaxon Schauer
    /// <summary>
    /// Holds essential save information for items
    /// </summary>
    [System.Serializable]
    public class ItemSaveData
    {
        public int amount;
        public int position;
        public string name;
        public int enhancementLevel;
        public ItemSaveData(int amount, string name, int position, int enhancementLevel)
        {
            this.amount = amount;
            this.name = name;
            this.position = position;
            this.enhancementLevel = enhancementLevel;
        }
    }
}
