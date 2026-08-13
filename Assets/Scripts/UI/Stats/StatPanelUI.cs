using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private Transform rowParent;
    [SerializeField] private StatRowUI rowPrefab;

    private readonly List<StatRowUI> spawnedRows = new List<StatRowUI>();

    private void Awake()
    {
        if (rowParent == null)
            rowParent = transform;
    }

    public void SetStats(IReadOnlyList<StatEntry> entries)
    {
        SetStats(entries, 0, -1);
    }

    public void SetStats(IReadOnlyList<StatEntry> entries, int startIndex, int count)
    {
        Clear();

        if (entries == null || rowParent == null || rowPrefab == null)
            return;

        int safeStartIndex = Mathf.Clamp(startIndex, 0, entries.Count);
        int availableCount = entries.Count - safeStartIndex;
        int safeCount = count < 0 ? availableCount : Mathf.Clamp(count, 0, availableCount);

        if (TryConsumeHeader(entries, ref safeStartIndex, ref safeCount) && safeCount <= 0)
            return;

        int endIndex = safeStartIndex + safeCount;
        for (int i = safeStartIndex; i < endIndex; i++)
        {
            StatEntry entry = entries[i];
            if (entry == null)
                continue;

            StatRowUI row = Instantiate(rowPrefab, rowParent);
            row.Set(entry);
            spawnedRows.Add(row);
        }
    }

    public void Clear()
    {
        if (headerText != null)
            headerText.text = "";

        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null)
                Destroy(spawnedRows[i].gameObject);
        }

        spawnedRows.Clear();
    }

    private bool TryConsumeHeader(IReadOnlyList<StatEntry> entries, ref int startIndex, ref int count)
    {
        if (headerText == null || count <= 0)
            return false;

        StatEntry firstEntry = entries[startIndex];
        if (firstEntry == null || firstEntry.Kind != StatEntryKind.Header)
            return false;

        headerText.text = firstEntry.Label;
        startIndex++;
        count--;
        return true;
    }
}
