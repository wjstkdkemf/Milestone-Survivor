using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour
{
   /* public TMP_Text powerUpNameText;
    public TMP_Text powerUpDescriptionText;
    public TMP_Text powerUpLevelText;
    public TMP_Text powerUpCostText;
    public Button purchaseButton;
    public Button refundButton;

    public PowerUpScriptableObject powerUp;
    private PowerUpManager powerUpManager;

   public void Initialize( PowerUpManager manager)
    {
        this.powerUpManager = manager;

        UpdateUI();
    }

    public void Purchase()
    {
        if (powerUpManager.PurchasePowerUp(powerUp))
        {
            UpdateUI();
        }
    }

    public void Refund()
    {
        if (powerUpManager.RefundPowerUp(powerUp))
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        int level = powerUpManager.GetPowerUpLevel(powerUp);
        powerUpNameText.text = powerUp.powerUpName;
        powerUpDescriptionText.text = powerUp.description;
        powerUpLevelText.text = $"Level: {level}/{powerUp.maxLevel}";

        if (level < powerUp.maxLevel)
        {
            float cost = powerUp.costPerLevel * Mathf.Pow(1.5f, level);
            powerUpCostText.text = $"Cost: {Mathf.RoundToInt(cost)} Gold";
            purchaseButton.interactable = true;
        }
        else
        {
            powerUpCostText.text = "Max Level";
            purchaseButton.interactable = false;
        }

        refundButton.interactable = level > 0;
    }*/
}
