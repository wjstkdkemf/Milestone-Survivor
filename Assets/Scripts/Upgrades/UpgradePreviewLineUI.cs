using TMPro;
using UnityEngine;

public class UpgradePreviewLineUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI currentValueText;
    [SerializeField] private TextMeshProUGUI arrowText;
    [SerializeField] private TextMeshProUGUI nextValueText;

    [Header("Optional Colors")]
    [SerializeField] private Color normalColor = new Color32(70, 55, 40, 255);
    [SerializeField] private Color increasedColor = new Color32(70, 145, 73, 255);
    [SerializeField] private Color decreasedColor = new Color32(190, 70, 60, 255);

    public void SetInfo(string statName, string currentValue, string nextValue)
    {
        if (statNameText != null)
            statNameText.text = string.IsNullOrEmpty(statName) ? "-" : statName;

        if (currentValueText != null)
            currentValueText.text = string.IsNullOrEmpty(currentValue) ? "-" : currentValue;

        if (arrowText != null)
            arrowText.text = "→";

        if (nextValueText != null)
            nextValueText.text = string.IsNullOrEmpty(nextValue) ? "-" : nextValue;

        ApplyDefaultColor();
    }

    public void SetInfo(UpgradePreviewLine line)
    {
        if (line == null)
        {
            SetInfo("-", "-", "-");
            return;
        }

        string statName = UpgradeLocalization.Get(line.StatNameKey, line.StatNameKey);
        string currentValue = line.LocalizeCurrentValue
            ? UpgradeLocalization.Get(line.CurrentValue, line.CurrentValue)
            : line.CurrentValue;
        string nextValue = line.LocalizeNextValue
            ? UpgradeLocalization.Get(line.NextValue, line.NextValue)
            : line.NextValue;

        SetInfo(statName, currentValue, nextValue);
    }

    private void ApplyDefaultColor()
    {
        if (statNameText != null)
            statNameText.color = normalColor;

        if (currentValueText != null)
            currentValueText.color = normalColor;

        if (arrowText != null)
            arrowText.color = normalColor;

        if (nextValueText != null)
            nextValueText.color = increasedColor;
    }
}
