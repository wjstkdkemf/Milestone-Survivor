using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeDescriptionPanel : MonoBehaviour
{
    [SerializeField] private RectTransform panelBG;
    [SerializeField] private RectTransform previewArea;
    [SerializeField] private Transform previewLineParent;
    [SerializeField] private UpgradePreviewLineUI previewLinePrefab;

    private readonly List<UpgradePreviewLineUI> previewLineUIs = new();

    public void RefreshPreviewLines(List<UpgradePreviewLine> lines)
    {
        ClearPreviewLines();

        if (lines != null && lines.Count > 0)
        {
            if (previewLinePrefab == null || previewLineParent == null)
            {
                Debug.LogError("Upgrade preview references are missing.", this);
                return;
            }

            foreach (UpgradePreviewLine line in lines)
            {
                UpgradePreviewLineUI lineUI =
                    Instantiate(previewLinePrefab, previewLineParent);

                lineUI.gameObject.SetActive(true);
                lineUI.SetInfo(line);
                previewLineUIs.Add(lineUI);
            }
        }

        ForceRebuildLayout();
    }

    private void ClearPreviewLines()
    {
        foreach (UpgradePreviewLineUI lineUI in previewLineUIs)
        {
            if (lineUI == null) continue;

            // Destroy completes later; exclude old rows from layout now.
            lineUI.gameObject.SetActive(false);
            Destroy(lineUI.gameObject);
        }

        previewLineUIs.Clear();
    }

    private void ForceRebuildLayout()
    {
        if (previewLineParent is RectTransform lineParentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(lineParentRect);
        if (previewArea != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(previewArea);
        if (panelBG != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelBG);
    }
}