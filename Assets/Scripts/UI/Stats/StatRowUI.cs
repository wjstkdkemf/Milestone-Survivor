using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatRowUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private GameObject divider;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.92f, 0.92f, 0.92f, 1f);
    [SerializeField] private Color positiveColor = new Color(0.29f, 0.84f, 0.43f, 1f);
    [SerializeField] private Color negativeColor = new Color(0.85f, 0.32f, 0.32f, 1f);
    [SerializeField] private Color headerColor = new Color(1f, 0.95f, 0.68f, 1f);
    [SerializeField] private Color dimColor = new Color(0.62f, 0.65f, 0.72f, 1f);

    public void Set(StatEntry entry)
    {
        if (entry == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (labelText != null)
            labelText.text = entry.Label;

        if (valueText != null)
        {
            valueText.text = entry.ValueText;
            valueText.gameObject.SetActive(entry.HasValue);
        }

        if (iconImage != null)
        {
            iconImage.sprite = entry.Icon;
            iconImage.enabled = entry.Icon != null;
        }

        if (divider != null)
            divider.SetActive(entry.Kind == StatEntryKind.Header);

        ApplyColor(entry.Kind);
    }

    private void ApplyColor(StatEntryKind kind)
    {
        Color color = GetColor(kind);

        if (labelText != null)
            labelText.color = color;

        if (valueText != null)
            valueText.color = color;
    }

    private Color GetColor(StatEntryKind kind)
    {
        switch (kind)
        {
            case StatEntryKind.Positive:
                return positiveColor;
            case StatEntryKind.Negative:
                return negativeColor;
            case StatEntryKind.Header:
                return headerColor;
            case StatEntryKind.Empty:
                return dimColor;
            default:
                return normalColor;
        }
    }
}
