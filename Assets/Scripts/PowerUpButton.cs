using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerUpButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text powerUpNameText;
    public Image Icon;


    public PowerUpScriptableObject powerUp;
    private PowerUpManager powerUpManager;
    public List<GameObject> UpgradePointsList;



    public string Description;

    // Scale & Animation settings
    private Vector3 normalScale;
    public float hoverScaleFactor = 1.1f;
    public float animationDuration = 0.2f;

    // Color & Glow effects
    private Graphic cardGraphic;  // For color (could be Image or TextMeshProUGUI)
    public Color hoverColor = Color.yellow; // Color to change to when hovered
    private Color originalColor; // Original color to reset to

    // Text Outline & Glow for TextMeshPro
    public TextMeshProUGUI cardText;
    public float glowOutlineWidth = 0.2f;
    private float originalOutlineWidth;
    public Color glowOutlineColor = Color.yellow;

    // Shadow Effect
    private Shadow shadowEffect; // For shadow manipulation
    public Vector2 hoverShadowOffset = new Vector2(5f, -5f);
    private Vector2 originalShadowOffset;

    // Sound effect on hover
    public AudioSource hoverSound;

    // Cursor
    public Texture2D customCursor;
    public bool IsSelected;

    public void Initialize( PowerUpManager manager)
    {
        normalScale = transform.localScale;

        // Initialize card graphic and colors
        cardGraphic = GetComponent<Graphic>();
        if (cardGraphic != null)
        {
            originalColor = cardGraphic.color;
        }

        // Initialize text outline if using TextMeshPro
        if (cardText != null)
        {
            originalOutlineWidth = cardText.outlineWidth;
        }

        Icon.sprite = powerUp.IconSprite;
        this.powerUpManager = manager;
        for (int i = 0; i < (powerUp.upgradeValues.Length); i++)
        {

            UpgradePointsList[i].SetActive(true);
        }
        UpdateUI();
    }

    public void Purchase()
    {
        if (powerUpManager.PurchasePowerUp(powerUp))
        {
            UpdateUI();
        }
    }



    public void UpdateUI()
    {
        for (int i = 0; i < powerUp.CurrentLevel; i++)
        {
            UpgradePointsList[i].transform.GetChild(0).gameObject.SetActive(true);
        }
        int level = powerUp.CurrentLevel;
        powerUpNameText.text = powerUp.powerUpName;
       // powerUpLevelText.text = $"Level: {level}/{powerUp.maxLevel}";

     /*   if (level < powerUp.maxLevel)
        {
            float cost = powerUp.costPerLevel[powerUp.CurrentLevel] ;
            powerUpCostText.text = $"Cost: {Mathf.RoundToInt(cost)} Gold";
            purchaseButton.interactable = true;
        }
        else
        {
            powerUpCostText.text = "Max Level";
            purchaseButton.interactable = false;
        }*/

     //   refundButton.interactable = level > 0;
    }
    public void ResetUI()
    {
        for (int i = 0; i < (powerUp.upgradeValues.Length); i++)
        {

            UpgradePointsList[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Play hover sound
        if (hoverSound != null)
        {
            hoverSound.Play();
        }

        // Smooth scale up using LeanTween
        LeanTween.scale(gameObject, normalScale * hoverScaleFactor, animationDuration).setEaseOutBack();

      /*  // Change color
        if (cardGraphic != null)
        {
            cardGraphic.color = hoverColor;
        }

        // Add glow outline for TextMeshPro
        if (cardText != null)
        {
            cardText.outlineWidth = glowOutlineWidth;
            cardText.outlineColor = glowOutlineColor;
        }

        // Increase shadow effect
        if (shadowEffect != null)
        {
            shadowEffect.effectDistance = hoverShadowOffset;
        }

        // Set custom cursor
        if (customCursor != null)
        {
            Cursor.SetCursor(customCursor, Vector2.zero, CursorMode.Auto);
        }*/
        //  TooltipManager.instance.SetInfo(Description);
      //  powerUpToolTip.SetInfo(powerUp);
    }

    // On hover exit, scale back to normal, reset color, remove glow, and reset shadow
    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsSelected == false)
            LeanTween.scale(gameObject, normalScale, animationDuration).setEaseInBack();
        /* // Reset color
         if (cardGraphic != null)
         {
             cardGraphic.color = originalColor;
         }

         // Remove glow outline for TextMeshPro
         if (cardText != null)
         {
             cardText.outlineWidth = originalOutlineWidth;
         }

         // Reset shadow effect
         if (shadowEffect != null)
         {
             shadowEffect.effectDistance = originalShadowOffset;
         }

         // Reset cursor to default
         Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
         powerUpToolTip.Hide();*/
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Selected();
    }
    public void Selected()
    {
        powerUpManager.DeselectOtherButtons();
        powerUpManager.SetInfo(powerUp, this);
        IsSelected = true;
        // Play hover sound
        if (hoverSound != null)
        {
            hoverSound.Play();
        }

        // Smooth scale up using LeanTween
        LeanTween.scale(gameObject, normalScale * hoverScaleFactor, animationDuration).setEaseOutBack();

        // Change color
        if (cardGraphic != null)
        {
            cardGraphic.color = hoverColor;
        }

        // Increase shadow effect
        if (shadowEffect != null)
        {
            shadowEffect.effectDistance = hoverShadowOffset;
        }

        // Set custom cursor
        if (customCursor != null)
        {
            Cursor.SetCursor(customCursor, Vector2.zero, CursorMode.Auto);
        }
    }
    public void DeSelected()
    {
        IsSelected = false;
        // Smooth scale down using LeanTween
        LeanTween.scale(gameObject, normalScale, animationDuration).setEaseInBack();

        // Reset color
        if (cardGraphic != null)
        {
            cardGraphic.color = originalColor;
        }
        // Reset shadow effect
        if (shadowEffect != null)
        {
            shadowEffect.effectDistance = originalShadowOffset;
        }

        // Reset cursor to default
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

}
/*   public PowerUpScriptableObject skill;
   public Button skillButton;
   public Image skillIcon;
   public TMP_Text buttonText; // Reference to the text on the button to show skill name and upgrade level
   private void Start()
   {
       // Register this button in the SkillManager
       if (PowerUpManager.Instance != null)
       {
           PowerUpManager.Instance.skillButtons = FindObjectsOfType<PowerUpButton>();
       }
       buttonText = transform.GetChild(0).GetComponent<TMP_Text>();
       skillButton.onClick.AddListener(OnSkillButtonClicked);
       UpdateSkillUI();
       UpdateButtonText();

   }

   public void UpdateSkillUI()
   {
       skillIcon.sprite = skill.skillIcon;
       if (!skill.isUnlocked)
           skillButton.interactable = PowerUpManager.Instance.CanUnlockSkill(skill);
       UpdateButtonText();
   }

   private void OnSkillButtonClicked()
   {
       if (PowerUpManager.Instance.CanUnlockSkill(skill))
       {
           PowerUpManager.Instance.UnlockSkill(skill);
           UpdateSkillUI();  // Update this button’s state
       }
       if (skill.isUnlocked)
       {
           PowerUpManager.Instance.UpgradeSkill(skill);
           UpdateButtonText();
       }
   }
   private void UpdateButtonText()
   {
       int currentUpgradeLevel = skill.CurrentUpgradeLevel;


       buttonText.text = currentUpgradeLevel.ToString();
       /* if(currentUpgradeLevel>= skill.Upgrades.Length)
            skillButton.interactable = SkillManager.Instance.CanUnlockSkill(skill);

   }


   public void PointerEnter()
   {
       //TooltipManager.instance.SetInfo(skill.skillName, skill.skillDescription, $"Current Level ({skill.CurrentUpgradeLevel}) \n {skill.Upgrades[skill.CurrentUpgradeLevel].value}\n", $"Next Level ({skill.CurrentUpgradeLevel + 1}) \n {skill.Upgrades[skill.CurrentUpgradeLevel + 1].value}", transform.position);
   }
   public void PointerExit()
   {
       TooltipManager.instance.HideTooltip();
   }
*/
//}