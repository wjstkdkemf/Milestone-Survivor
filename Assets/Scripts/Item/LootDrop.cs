using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LootEntry
{
    public GameObject itemPrefab;
    [Range(0, 100)]
    public float dropChance;
}

public class LootDrop : MonoBehaviour
{
    public List<LootEntry> lootTable = new List<LootEntry>();

    public void DropLoot()
    {
        foreach (var entry in lootTable)
        {
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= entry.dropChance)
            {
                // Slightly randomize the drop position
                Vector3 position = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(entry.itemPrefab, position, Quaternion.identity);
            }
        }
    }
}
