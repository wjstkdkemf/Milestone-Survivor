using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PowerUpManager : MonoBehaviour
{
 
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
    private string saveFilePath;

    public bool InGame;
    private void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/powerUps.json";
        LoadPowerUps();
    }
    private void Start()
    {
        if (InGame)
            return;
        //  LoadProgress();
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
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            SavePowerUps();
        
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            LoadPowerUps();
        }
    }
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


        powerUpButton. Purchase();
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

        float cost = powerUp.costPerLevel[powerUp.CurrentLevel]; // Exponential scaling
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
       // playerStats.ResetPowerUps();
    }

    public void SavePowerUps()
    {
        Debug.Log("savePowerUps");
        List<PowerUpData> powerUpDataList = new List<PowerUpData>();

        foreach (var powerUp in powerUps)
        {
            powerUpDataList.Add(new PowerUpData(powerUp));
        }

        string json = JsonUtility.ToJson(new SerializablePowerUpList(powerUpDataList), true);
        System.IO.File.WriteAllText(saveFilePath, json);
    }
    public void LoadPowerUps()
    {
        if (!System.IO.File.Exists(saveFilePath))
            return;
        Debug.Log("loadPowerUps");
        string json = System.IO.File.ReadAllText(saveFilePath);
        SerializablePowerUpList loadedData = JsonUtility.FromJson<SerializablePowerUpList>(json);

        foreach (var loadedPowerUp in loadedData.powerUps)
        {
            foreach (var powerUp in powerUps)
            {
                if (powerUp.powerUpName == loadedPowerUp.powerUpName)
                {
                    powerUp.CurrentLevel = loadedPowerUp.currentLevel;
                    powerUp.upgradeValues = loadedPowerUp.upgradeValues;
                    powerUp.costPerLevel = loadedPowerUp.costPerLevel;
                    powerUp.isPercentage = loadedPowerUp.isPercentage;
                    break;
                }
            }
        }
    }
}
[System.Serializable]
public class PowerUpData
{
    public string powerUpName;
    public PowerUpType powerUpType;
    public int currentLevel;
    public float[] upgradeValues;
    public float[] costPerLevel;
    public bool isPercentage;

    // Constructor to populate from the ScriptableObject
    public PowerUpData(PowerUpScriptableObject powerUp)
    {
        powerUpName = powerUp.powerUpName;
        powerUpType = powerUp.powerUpType;
        currentLevel = powerUp.CurrentLevel;
        upgradeValues = powerUp.upgradeValues;
        costPerLevel = powerUp.costPerLevel;
        isPercentage = powerUp.isPercentage;
    }
}
[System.Serializable]
public class SerializablePowerUpList
{
    public List<PowerUpData> powerUps;

    public SerializablePowerUpList(List<PowerUpData> powerUps)
    {
        this.powerUps = powerUps;
    }
}