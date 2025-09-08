using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryData
{
    public List<string> itemNames = new List<string>();
    public List<int> itemCounts = new List<int>();

    public InventoryData(Dictionary<string, int> items)
    {
        foreach (var pair in items)
        {
            itemNames.Add(pair.Key);
            itemCounts.Add(pair.Value);
        }
    }

    public Dictionary<string, int> ToDictionary()
    {
        var dict = new Dictionary<string, int>();
        for (int i = 0; i < itemNames.Count; i++)
        {
            dict[itemNames[i]] = itemCounts[i];
        }
        return dict;
    }
}
