using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Stat Icon Database")]
public class StatIconDatabase : ScriptableObject
{
    [SerializeField] private List<StatIconEntry> entries;

    private Dictionary<string, Sprite> lookup;

    public Sprite GetIcon(string statKey)
    {
        if (string.IsNullOrEmpty(statKey))
            return null;

        EnsureLookup();
        return lookup.TryGetValue(statKey, out Sprite icon) ? icon : null;
    }

    private void EnsureLookup()
    {
        if (lookup != null)
            return;

        lookup = new Dictionary<string, Sprite>();
        if (entries == null)
            return;

        foreach (StatIconEntry entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.statKey))
                continue;

            if (lookup.ContainsKey(entry.statKey))
            {
                Debug.LogWarning($"[StatIconDatabase] Duplicate stat key '{entry.statKey}' in '{name}'. The first icon will be used.");
                continue;
            }

            lookup.Add(entry.statKey, entry.icon);
        }
    }

    private void OnValidate()
    {
        lookup = null;
    }
}

[System.Serializable]
public class StatIconEntry
{
    public string statKey;
    public Sprite icon;
}
