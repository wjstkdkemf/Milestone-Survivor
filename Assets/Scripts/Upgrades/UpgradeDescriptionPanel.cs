using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

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
    private UpgradeScriptableObject currentUpgrade;
    private WeaponBase currentWeapon;

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    public void Show(UpgradeScriptableObject upgrade, WeaponBase weapon)
    {
        if (upgrade == null)
            return;

        currentUpgrade = upgrade;
        currentWeapon = weapon;

        if (root != null)
            root.SetActive(true);

        UpgradePreviewData preview = null;

        if (weapon != null)
        {
            preview = weapon.GetUpgradePreview(upgrade);
        }

        if (preview == null)
            preview = UpgradePreviewFactory.CreateFallbackPreview(upgrade);

        if (preview == null)
            return;

        /*if (!preview.Lines.Exists(line => line.StatNameKey == "upgrade.stat.level"))
            preview.Lines.Add(CreateLevelPreviewLine(upgrade));*/

        if (upgrade.MaxPoints > 0)
        {
            int currentPoints = upgrade.Points;
            int nextPoints = Mathf.Min(currentPoints + 1, upgrade.MaxPoints);

            int levelIndex = preview.Lines.FindIndex(
                line => line.StatNameKey == "upgrade.stat.level"
            );

            int insertIndex = levelIndex >= 0 ? levelIndex + 1 : 0;

            preview.Lines.Insert(insertIndex, new UpgradePreviewLine(
                "upgrade.stat.investment_count",
                $"{currentPoints}/{upgrade.MaxPoints}",
                $"{nextPoints}/{upgrade.MaxPoints}"
            ));
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
        int currentLevel = upgrade.Points;
        int nextLevel = currentLevel + 1;
        bool isMaxLevel = upgrade.MaxPoints > 0 && upgrade.Points >= upgrade.MaxPoints;

        string nextLevelText = isMaxLevel
            ? UpgradeLocalization.Get("upgrade.value.max", "MAX")
            : $"Lv.{nextLevel}";

        return new UpgradePreviewLine(
            "upgrade.stat.level",
            $"Lv.{currentLevel}",
            nextLevelText
        );
    }

    private void HandleLocaleChanged(Locale locale)
    {
        if (currentUpgrade != null && root != null && root.activeSelf)
            Show(currentUpgrade, currentWeapon);
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
