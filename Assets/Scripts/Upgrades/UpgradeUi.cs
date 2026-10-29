using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class UpgradeUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UpgradeScriptableObject Upgrade;

    [SerializeField] private TMP_Text Title;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private Image Icon;
    [SerializeField] private string UpgradeName;
    [SerializeField] private TMP_Text UpgradeLevel;
    public List<GameObject> UpgradePointsList;

    [SerializeField] private GameObject hoverBorder;
    [SerializeField] private GameObject selectedBorder;

    [Header("Grade Visuals")]
    [SerializeField] private GradeVisualSet[] gradeVisualSets;
    [Header("Card Images")]
    [SerializeField] private Image cardImage;
    [SerializeField] private List<Image> dividerImages = new();

    [Header("Point Images")]
    [SerializeField] private GameObject pointsRoot;
    [SerializeField] private List<Image> pointImages = new();

    private GradeVisualSet currentGradeVisuals;
    private bool isSelected;
    [SerializeField] private UpgradeDescriptionPanel descriptionPanel;
    [SerializeField] private TMP_Text ShortDescription;
    [SerializeField] private TMP_Text GradeText;
    [SerializeField] private PlayerWeaponController playerWeaponController;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    public void SetInfo(
        UpgradeScriptableObject info,
        PlayerWeaponController weaponController = null
    )
    {
        Upgrade = info;
        if (weaponController != null)
            playerWeaponController = weaponController;
        else if (playerWeaponController == null && UpgradeManager.Instance != null)
            playerWeaponController = UpgradeManager.Instance.playerWeaponController;
        
        if (Icon != null)
            Icon.sprite = Upgrade.Icon;

        if (UpgradeLevel != null)
        {
            if (Upgrade.IsWeaponUpgrade)
            {
                if (playerWeaponController != null)
                {
                    var data = Upgrade.linkedWeaponData;

                    int learned = playerWeaponController.GetLearnedLevel(data);
                    int equipment = playerWeaponController.GetEquipmentLevel(data);

                    UpgradeLevel.text = $"Lv.{learned + equipment + 1}";
                }
                else
                {
                    UpgradeLevel.text = "-";
                }
            }
            else
            {
                UpgradeLevel.text = $"Lv.{Upgrade.Points + 1}";
            }
        }

        if (hoverBorder != null)
            hoverBorder.SetActive(false);
        ApplyGradeVisuals();
        RefreshPoints();
        RefreshLocalizedText();
    }
    public void UpgradeFunction()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeSelected(Upgrade);
        }

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected && hoverBorder != null)
            hoverBorder.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverBorder != null)
            hoverBorder.SetActive(false);
    }

    private void HandleLocaleChanged(Locale locale)
    {
        RefreshLocalizedText();
    }

    private void RefreshLocalizedText()
    {
        if (Upgrade == null) return;

        WeaponBase weapon = null;
        if (Upgrade.IsWeaponUpgrade && playerWeaponController != null)
        {
            playerWeaponController.TryGetWeapon(
                Upgrade.linkedWeaponData,
                out weapon,
                includeEquipmentWeapons: true
            );
        }

        UpgradePreviewData preview = weapon != null
            ? weapon.GetUpgradePreview(Upgrade)
            : null;

        preview ??= UpgradePreviewFactory.CreateFallbackPreview(Upgrade);

        if (Title != null)
            Title.text = Upgrade.GetLocalizedTitle();

        if (ShortDescription != null)
            ShortDescription.text = preview?.ShortDescription ?? "";

        if (Description != null)
            Description.text = preview?.Description ?? "";

        if (GradeText != null)
        {
            GradeText.text = GradeColorUtility.GetDisplayName(Upgrade.Grade);
            GradeText.color = GradeColorUtility.GetColor(Upgrade.Grade);
        }

        if (descriptionPanel != null)
            descriptionPanel.RefreshPreviewLines(preview?.Lines);
    }
    private void ApplyGradeVisuals()
    {
        currentGradeVisuals = null;

        if (Upgrade == null) return;

        if (gradeVisualSets != null)
        {
            foreach (var visuals in gradeVisualSets)
            {
                if (visuals == null || visuals.grade != Upgrade.Grade)
                    continue;

                currentGradeVisuals = visuals;
                break;
            }
        }

        if (currentGradeVisuals == null)
        {
            Debug.LogWarning($"Missing grade visuals: {Upgrade.Grade}", this);
            return;
        }

        if (cardImage != null)
        {
            cardImage.sprite = currentGradeVisuals.cardSprite;
            cardImage.color = Color.white;
        }

        foreach (Image divider in dividerImages)
        {
            if (divider == null) continue;

            divider.sprite = currentGradeVisuals.dividerSprite;
            divider.color = Color.white;
        }
    }
    private void RefreshPoints()
    {
        if (pointsRoot == null) return;

        bool canDisplay = Upgrade != null &&
                        currentGradeVisuals != null &&
                        Upgrade.MaxPoints > 0 &&
                        Upgrade.MaxPoints <= pointImages.Count;

        pointsRoot.SetActive(canDisplay);
        if (!canDisplay) return;

        if (currentGradeVisuals.emptyPointSprite == null ||
            currentGradeVisuals.filledPointSprite == null)
        {
            pointsRoot.SetActive(false);
            Debug.LogWarning($"Missing point sprites: {Upgrade.Grade}", this);
            return;
        }

        int invested = Mathf.Clamp(Upgrade.Points, 0, Upgrade.MaxPoints);

        for (int i = 0; i < pointImages.Count; i++)
        {
            Image gem = pointImages[i];
            if (gem == null) continue;

            bool visible = i < Upgrade.MaxPoints;
            gem.gameObject.SetActive(visible);
            if (!visible) continue;

            gem.sprite = i < invested
                ? currentGradeVisuals.filledPointSprite
                : currentGradeVisuals.emptyPointSprite;

            gem.color = Color.white;
        }
    }
    [System.Serializable]
    private class GradeVisualSet
    {
        public UpgradeGrade grade;

        [Header("Card")]
        public Sprite cardSprite;
        public Sprite dividerSprite;

        [Header("Points")]
        public Sprite emptyPointSprite;
        public Sprite filledPointSprite;
    }

    // void ClearEncount()
    // {
    //     GameObject Encounter = GameObject.FindWithTag("EnCount");
    //     Debug.Log("체크포인트 1");
    //     if (Encounter != null)
    //     {
    //         EnCounterSystem enCounterSystem = Encounter.GetComponent<EnCounterSystem>();
    //         if (enCounterSystem != null)
    //         {
    //             enCounterSystem.ClearEncount();
    //         }
    //     }
    // }
}
