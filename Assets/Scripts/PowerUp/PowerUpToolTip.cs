using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpToolTip : MonoBehaviour
{
    public GameObject panle;
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public Image icon;


    public void SetInfo(PowerUpScriptableObject info)
    {
        panle.SetActive(true);
        nameText.text = info.GetLocalizedName();
        string current = info.FormatValue(info.upgradeValues[info.CurrentLevel]);
        string maximum = info.FormatValue(info.upgradeValues[info.upgradeValues.Length - 1]);

        descriptionText.text =
            $"{info.GetLocalizedDescription()} by {current}, (max {maximum})";
        costText.text = info.costPerLevel[info.CurrentLevel].ToString();
        icon.sprite = info.IconSprite;
    }
    public void Hide()
    {
        panle.SetActive(false);
    }
}
