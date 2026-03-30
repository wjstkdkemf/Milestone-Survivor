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
                GameObject prefabToProcess = entry.itemPrefab;
                if (prefabToProcess == null) continue;

                // Check if the prefab is a 'Loot Container' by looking for the LootTable component.
                LootTable lootContainer = prefabToProcess.GetComponent<LootTable>();

                if (lootContainer != null)
                {
                    // This is a container. Instantiate it, have it drop an item from its own table,
                    // and then destroy the container instance itself.
                    GameObject containerInstance = Instantiate(prefabToProcess, transform.position, Quaternion.identity);
                    containerInstance.GetComponent<LootTable>().DropSingleItem();
                    Destroy(containerInstance);
                }
                else
                {
                    // This is a regular item. Instantiate it directly.
                    Vector3 position = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                    Instantiate(prefabToProcess, position, Quaternion.identity);
                }
            }
        }
    }
}
