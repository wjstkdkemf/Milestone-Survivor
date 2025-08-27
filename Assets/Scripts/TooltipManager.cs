using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager instance;

    public GameObject ToolTipObject; // Tooltip object (Panel/UI element)
    public GameObject ToolTipObjectPostion; // Tooltip follow position
    public TMP_Text ToolTipText;
    public string Description;
    public Vector3 offSet; // Offset from the mouse position

    private CanvasGroup tooltipCanvasGroup; // For fade effect
    public float fadeDuration = 0.2f; // Fade animation duration
    public float scaleDuration = 0.2f; // Scale animation duration
    public Vector3 startScale = new Vector3(0.8f, 0.8f, 0.8f); // Starting scale (slightly smaller)

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Get CanvasGroup for fade effect
        tooltipCanvasGroup = ToolTipObject.GetComponent<CanvasGroup>();
        if (tooltipCanvasGroup != null)
        {
            tooltipCanvasGroup.alpha = 0; // Start as invisible
        }

        // Set initial scale to a smaller size
        ToolTipObject.transform.localScale = startScale;
        // Hide tooltip on start
        ToolTipObject.SetActive(false);
    }

    void Update()
    {
        // Make the tooltip follow the mouse with an offset
        if (ToolTipObjectPostion.activeInHierarchy)
        {
            ToolTipObjectPostion.transform.position = Input.mousePosition + offSet;
        }
    }

    // Function to set info and display upgrades in the tooltip
    public void SetInfo(string info)
    {
    
        Description = info;
        // Show the tooltip with animations
        ShowTooltip();
    }

    // Show the tooltip with fade-in and scale-up effect
    public void ShowTooltip()
    {
        ToolTipText.text = Description;
        ToolTipObject.SetActive(true);

        // Reset scale and alpha to initial values
        ToolTipObject.transform.localScale = startScale;
        tooltipCanvasGroup.alpha = 0;

        // Animate the scale and fade-in effect using LeanTween
        LeanTween.scale(ToolTipObject, Vector3.one, scaleDuration).setEaseOutBack();
        LeanTween.alphaCanvas(tooltipCanvasGroup, 1, fadeDuration).setEaseOutQuad();
    }

    // Hide the tooltip with fade-out and scale-down effect
    public void HideTooltip()
    {
        // Animate fade-out and scale-down effect
        LeanTween.alphaCanvas(tooltipCanvasGroup, 0, fadeDuration).setEaseInQuad();
        LeanTween.scale(ToolTipObject, startScale, scaleDuration).setEaseInBack().setOnComplete(() =>
        {
            ToolTipObject.SetActive(false);
        });
    }
}