using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    private bool isSelected;
    public UpgradeDescriptionPanel descriptionPanel;
    [SerializeField] private PlayerWeaponController playerWeaponController;

    public void SetInfo(
        UpgradeScriptableObject info,
        UpgradeDescriptionPanel panel,
        PlayerWeaponController weaponController = null
    )
    {
        Upgrade = info;
        descriptionPanel = panel;
        if (weaponController != null)
            playerWeaponController = weaponController;
        else if (playerWeaponController == null && UpgradeManager.Instance != null)
            playerWeaponController = UpgradeManager.Instance.playerWeaponController;
        
        if (Title != null)
            Title.text = Upgrade.Title;

        if (Icon != null)
            Icon.sprite = Upgrade.Icon;

        if (Description != null)
            Description.text = Upgrade.Description;

        if (UpgradeLevel != null)
            UpgradeLevel.text = "Lv." + (Upgrade.Points + 1).ToString();

        if (hoverBorder != null)
            hoverBorder.SetActive(false);
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

        WeaponBase weapon = null;

        if (Upgrade != null && Upgrade.linkedWeaponData != null && playerWeaponController != null)
        {
            playerWeaponController.TryGetWeapon(Upgrade.linkedWeaponData, out weapon);
        }

        if(descriptionPanel != null)
            descriptionPanel.Show(Upgrade, weapon);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverBorder != null)
            hoverBorder.SetActive(false);

        if (!isSelected && descriptionPanel != null)
            descriptionPanel.Hide();
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
