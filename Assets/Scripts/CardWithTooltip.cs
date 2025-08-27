using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardWithTooltip : MonoBehaviour
{
    // Reference to the Tooltip UI (Text or Panel)
    public GameObject tooltip;
    public TextMeshProUGUI tooltipText; // Tooltip Text for displaying card info

    // Tooltip Animation Settings
    public float tooltipFadeDuration =   0.2f; // Duration for fade in/out animation
    private CanvasGroup tooltipCanvasGroup; // To control fade effect

    // Tooltip Position Offset
    public Vector3 tooltipOffset = new Vector3(0, 100, 0); // Offset from card

    // Card info for the tooltip
    public string cardInfo = "This is a cool card!";

    void Start()
    {
        if (tooltip != null)
        {
            // Get the CanvasGroup component for controlling the tooltip's opacity
            tooltipCanvasGroup = tooltip.GetComponent<CanvasGroup>();

            if (tooltipCanvasGroup != null)
            {
                // Set initial tooltip state (hidden)
                tooltipCanvasGroup.alpha = 0;
            }

            // Ensure the tooltip is hidden initially
            tooltip.SetActive(false);
        }
    }

    // On hover, display the tooltip
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Set tooltip text to cardInfo
        if (tooltipText != null)
        {
            tooltipText.text = cardInfo;
        }

        // Position the tooltip near the card with an offset
        tooltip.transform.position = transform.position + tooltipOffset;

        // Display and fade in the tooltip
        tooltip.SetActive(true);

        // Use LeanTween to animate the fade-in of the tooltip
        if (tooltipCanvasGroup != null)
        {
            LeanTween.alphaCanvas(tooltipCanvasGroup, 1, tooltipFadeDuration).setEaseOutQuad();
        }
    }

    // On hover exit, hide the tooltip
    public void OnPointerExit(PointerEventData eventData)
    {
        // Fade out the tooltip and hide it after the animation completes
        if (tooltipCanvasGroup != null)
        {
            LeanTween.alphaCanvas(tooltipCanvasGroup, 0, tooltipFadeDuration).setEaseInQuad().setOnComplete(() =>
            {
                tooltip.SetActive(false);
            });
        }
        else
        {
            // If no CanvasGroup, just deactivate the tooltip
            tooltip.SetActive(false);
        }
    }
}