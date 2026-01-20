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
        nameText.text = info.powerUpName;
        string percentage = info.isPercentage ? "%" : "";
        descriptionText.text = $"{info.description} by {info.upgradeValues[info.CurrentLevel] } {percentage} per level Up , (max {info.upgradeValues[info.upgradeValues.Length-1] } { percentage} )";
        costText.text = info.costPerLevel[info.CurrentLevel].ToString();
        icon.sprite = info.IconSprite;
    }
    public void Hide()
    {
        panle.SetActive(false);
    }
}
