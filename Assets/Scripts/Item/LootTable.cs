using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LootTableItem
{
    public GameObject itemPrefab;
    public float dropChance; 
}

public class LootTable : MonoBehaviour
{
    public string LootTableID;
    public List<LootTableItem> lootItems = new List<LootTableItem>();

    public void DropSingleItem()
    {
        float totalWeight = 0f;
        foreach (var item in lootItems)
        {
            totalWeight += item.dropChance;
        }

        if (totalWeight <= 0) return;

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var item in lootItems)
        {
            cumulativeWeight += item.dropChance;
            if (randomValue <= cumulativeWeight)
            {
                if (item.itemPrefab != null)
                {
                    Vector3 position = transform.position;// + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0)
                    Instantiate(item.itemPrefab, position, Quaternion.identity);
                }
                return; 
            }
        }
    }
    public LootTableItem QuestDrop()
    {
        float totalWeight = 0f;
        foreach (var item in lootItems)
        {
            totalWeight += item.dropChance;
        }

        if (totalWeight <= 0) return null;

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var item in lootItems)
        {
            cumulativeWeight += item.dropChance;
            if (randomValue <= cumulativeWeight)
            {
                if (item != null)
                {
                    return item;
                }
                return null; 
            }
        }
        return null;
    }
}