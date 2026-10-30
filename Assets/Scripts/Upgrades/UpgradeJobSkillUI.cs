using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeJobSkillUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Color unmetColor = new Color32(150, 60, 45, 255);
    [SerializeField] private Color metColor = new Color32(50, 120, 65, 255);

    public void SetRequirement(
        UpgradeScriptableObject upgrade,
        int currentPoints,
        int requiredPoints)
    {
        SetIcon(upgrade);

        progressText.gameObject.SetActive(true);
        progressText.text = $"{currentPoints}/{requiredPoints}";
        progressText.color = currentPoints >= requiredPoints
            ? metColor
            : unmetColor;
    }

    public void SetReward(UpgradeScriptableObject upgrade)
    {
        SetIcon(upgrade);
        progressText.gameObject.SetActive(false);
    }

    private void SetIcon(UpgradeScriptableObject upgrade)
    {
        icon.sprite = upgrade != null ? upgrade.Icon : null;
        icon.color = Color.white;
        icon.enabled = icon.sprite != null;
    }
}