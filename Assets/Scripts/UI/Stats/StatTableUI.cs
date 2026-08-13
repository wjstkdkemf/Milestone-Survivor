using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;


public class StatTableUI : MonoBehaviour
{
    [FormerlySerializedAs("HeadTitle")]
    [SerializeField] private TextMeshProUGUI headTitle;
    [SerializeField] private StatPanelUI leftColumn;
    [SerializeField] private StatPanelUI rightColumn;
    [SerializeField] private bool balanceColumns = true;

    public void SetStats(IReadOnlyList<StatEntry> stats)
    {
        Clear();

        if (stats == null)
            return;

        if (leftColumn == null && rightColumn == null)
            return;

        int startIndex = 0;
        int count = stats.Count;
        TryConsumeHeader(stats, ref startIndex, ref count);

        if (rightColumn == null)
        {
            leftColumn?.SetStats(stats, startIndex, count);
            return;
        }

        if (leftColumn == null)
        {
            rightColumn.SetStats(stats, startIndex, count);
            return;
        }

        int leftCount = balanceColumns
            ? Mathf.CeilToInt(count / 2f)
            : count;
        int rightCount = count - leftCount;

        leftColumn.SetStats(stats, startIndex, leftCount);
        rightColumn.SetStats(stats, startIndex + leftCount, rightCount);
    }

    public void Clear()
    {
        if (headTitle != null)
            headTitle.text = "";

        if (leftColumn != null)
            leftColumn.Clear();

        if (rightColumn != null)
            rightColumn.Clear();
    }

    private bool TryConsumeHeader(IReadOnlyList<StatEntry> entries, ref int startIndex, ref int count)
    {
        if (headTitle == null || count <= 0)
            return false;

        headTitle.text = "";

        StatEntry firstEntry = entries[startIndex];
        if (firstEntry == null || firstEntry.Kind != StatEntryKind.Header)
            return false;

        headTitle.text = firstEntry.Label;
        startIndex++;
        count--;
        return true;
    }
}
