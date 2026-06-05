using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeDescriptionPanel : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI ShortdescriptionText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI GradeText;

    [SerializeField] private RectTransform panelBG;
    [SerializeField] private RectTransform previewArea;
    [SerializeField] private Transform previewLineParent;
    [SerializeField] private UpgradePreviewLineUI previewLinePrefab;

    private readonly List<UpgradePreviewLineUI> previewLineUIs = new();

    private void Awake()
    {
        Hide();
    }

    public void Show(UpgradeScriptableObject upgrade, WeaponBase weapon)
    {
        if (upgrade == null)
            return;

        if (root != null)
            root.SetActive(true);

        UpgradePreviewData preview = null;
        bool usesWeaponPreview = false;

        if (weapon != null)
        {
            preview = weapon.GetUpgradePreview(upgrade);
            usesWeaponPreview = preview != null;
        }

        if (preview == null)
            preview = UpgradePreviewFactory.CreateFallbackPreview(upgrade, PlayerStats.Instance);

        if (preview != null && !usesWeaponPreview)
        {
            preview.Lines.Add(CreateLevelPreviewLine(upgrade));
        }
        else if (preview == null)
        {
            return;
        }

        if(ShortdescriptionText != null)
            ShortdescriptionText.text = preview.ShortDescription;
        
        if(descriptionText != null)
            descriptionText.text = preview.Description;

        if(GradeText != null)
        {
            string gradeName = GradeColorUtility.GetDisplayName(upgrade.Grade);
            string colorHex = GradeColorUtility.GetHexColor(upgrade.Grade);

            GradeText.text = $"<color=#{colorHex}>{gradeName}</color>";
        }

        RefreshPreviewLines(preview.Lines);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    private string GetNextEffectText(UpgradeScriptableObject upgrade)
    {
        return "다음 레벨 효과를 표시";
    }
    public void RefreshPreviewLines(List<UpgradePreviewLine> lines)
    {
        ClearPreviewLines();

        if (lines != null)
        {
            foreach (UpgradePreviewLine line in lines)
            {
                UpgradePreviewLineUI lineUI = Instantiate(previewLinePrefab, previewLineParent);
                lineUI.SetInfo(line);
                previewLineUIs.Add(lineUI);
            }
        }

        ForceRebuildLayout();
    }

    private void ClearPreviewLines()
    {
        for (int i = 0; i < previewLineUIs.Count; i++)
        {
            if (previewLineUIs[i] != null)
                Destroy(previewLineUIs[i].gameObject);
        }

        previewLineUIs.Clear();
    }
    private UpgradePreviewLine CreateLevelPreviewLine(UpgradeScriptableObject upgrade)
    {
        int currentLevel = upgrade.Points + 1;
        bool isMaxLevel = upgrade.MaxPoints > 0 && upgrade.Points >= upgrade.MaxPoints - 1;

        string nextLevelText = isMaxLevel
            ? "MAX"
            : $"Lv.{currentLevel + 1}";

        return new UpgradePreviewLine(
            "레벨",
            $"Lv.{currentLevel}",
            nextLevelText
        );
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
