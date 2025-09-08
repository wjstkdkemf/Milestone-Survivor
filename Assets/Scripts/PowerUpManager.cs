using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set;}
    public List<PowerUpScriptableObject> powerUps;
    public List<PowerUpButton> powerUpButtons;
    public PlayerStats playerStats;

    public GameObject panle;
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public GameObject BuyButtons;
    public Image PowerUpIcon;
    public PowerUpButton powerUpButton;

    public bool InGame;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (InGame)
            return;

        foreach (PowerUpButton button in powerUpButtons)
        {
            button.Initialize(this);
        }
        Invoke("delayedStart", .2f);
    }
    void delayedStart()
    {
        powerUpButtons[0].Selected();
    }

    // --- Integration with SaveLoadManager ---

    public PowerUpSaveData GetSaveData()
    {
        // Create dictionary from the list of scriptable objects
        var powerUpLevels = new Dictionary<PowerUpType, int>();
        foreach (var powerUp in powerUps)
        {
            powerUpLevels[powerUp.powerUpType] = powerUp.CurrentLevel;
        }
        return new PowerUpSaveData(powerUpLevels);
    }

    public void LoadData(PowerUpSaveData data)
    {
        if (data == null) 
        {
            Debug.LogWarning("PowerUpSaveData is null, cannot load data.");
            return;
        }

        var powerUpLevels = data.ToDictionary();

        // Update ScriptableObjects from loaded data
        foreach (var powerUp in powerUps)
        {
            if (powerUpLevels.ContainsKey(powerUp.powerUpType))
            {
                powerUp.CurrentLevel = powerUpLevels[powerUp.powerUpType];
            }
        }

        // Update UI after loading
        foreach (var button in powerUpButtons)
        {
            button.UpdateUI();
        }
        
        Debug.Log("PowerUp data loaded and UI updated.");
    }

    // --- Existing UI and Purchase Logic ---

    public void SetInfo(PowerUpScriptableObject info, PowerUpButton button)
    {
        powerUpButton = button;
        panle.SetActive(true);
        nameText.text = info.powerUpName;
        descriptionText.text = info.description;
        if (info.CurrentLevel < info.upgradeValues.Length)
            costText.text = info.costPerLevel[info.CurrentLevel].ToString();
        else
            costText.text = "";
        PowerUpIcon.sprite = info.IconSprite;
        if (powerUpButton.powerUp.CurrentLevel < powerUpButton.powerUp.upgradeValues.Length)
            BuyButtons.SetActive(true);
        else
            BuyButtons.SetActive(false);
    }
    public void Purchase()
    {
        powerUpButton.Purchase();
        powerUpButton.UpdateUI();
        if (powerUpButton.powerUp.CurrentLevel >= powerUpButton.powerUp.upgradeValues.Length)
            BuyButtons.SetActive(false);
    }
    public void DeselectOtherButtons()
    {
        foreach (PowerUpButton button in powerUpButtons)
        {
            button.DeSelected();
        }
    }
    public bool PurchasePowerUp(PowerUpScriptableObject powerUp)
    {
        if (powerUp.CurrentLevel >= powerUp.upgradeValues.Length)
        {
            Debug.Log("Power-up is already maxed out!");
            return false;
        }

        float cost = powerUp.costPerLevel[powerUp.CurrentLevel];
        if (playerStats.GoldAmount < cost)
        {
            Debug.Log("Not enough gold!");
            return false;
        }

        playerStats.GoldAmount -= Mathf.RoundToInt(cost);
        powerUp.CurrentLevel++;
        Debug.Log($"Purchased {powerUp.powerUpName} Level {powerUp.CurrentLevel}!");
        return true;
    }

    public void RefundPowerUp()
    {
        foreach (PowerUpButton powerup in powerUpButtons)
        {
            float amount = 0;

            for (int i = 0; i < powerup.powerUp.CurrentLevel; i++)
            {
                amount += powerup.powerUp.costPerLevel[i];
            }

            PlayerStats.Instance.GoldAmount += Mathf.RoundToInt(amount);
            powerup.powerUp.CurrentLevel=0;
            powerup.ResetUI();
        }
    }
}
