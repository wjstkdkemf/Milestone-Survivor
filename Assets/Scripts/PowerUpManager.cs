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

    public GameObject panel;
    public TMP_Text MyGoldText;
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public GameObject BuyButtons;
    public Image PowerUpIcon;
    public PowerUpButton powerUpButton;
    private PowerUpButton currentSelectedButton;

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

        playerStats = FindAnyObjectByType<PlayerStats>();

        UpdateGoldUI();

        if (powerUpButtons.Count > 0)
        {
            powerUpButtons[0].Selected();
        }
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
            Debug.LogWarning("저장된 파워업 데이터가 없어 기본값으로 초기화합니다.");

            // 모든 파워업 레벨을 0으로 초기화
            foreach (var powerUp in powerUps)
            {
                powerUp.CurrentLevel = 0;
            }

            // UI가 초기화된 상태를 반영하도록 업데이트
            foreach (var button in powerUpButtons)
            {
                button.UpdateUI();
            }
            UpdateGoldUI();
            // 초기화 후 함수 종료
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

        UpdateGoldUI();
        Debug.Log("PowerUp data loaded and UI updated.");
    }

    // --- Existing UI and Purchase Logic ---

    public void SetInfo(PowerUpButton button)
    {
        currentSelectedButton = button;

        if(panel != null)
            UpdateDetailPanel(button.powerUp);
    }
    private void UpdateDetailPanel(PowerUpScriptableObject info)
    {
        panel.SetActive(true);
        nameText.text = info.powerUpName;
        descriptionText.text = info.description;
        PowerUpIcon.sprite = info.IconSprite;

        // Max Level 체크
        bool isMaxLevel = info.CurrentLevel >= info.upgradeValues.Length;

        if (!isMaxLevel)
        {
            costText.text = info.costPerLevel[info.CurrentLevel].ToString();
            BuyButtons.SetActive(true);
        }
        else
        {
            costText.text = "MAX";
            BuyButtons.SetActive(false);
        }

        UpdateGoldUI();
    }
    public void Purchase()
    {
        if (currentSelectedButton == null) return;

        PowerUpScriptableObject powerUp = currentSelectedButton.powerUp;

        // 1. 만렙 체크
        if (powerUp.CurrentLevel >= powerUp.upgradeValues.Length)
        {
            Debug.Log("Already Max Level!");
            return;
        }

        // 2. 골드 체크
        float cost = powerUp.costPerLevel[powerUp.CurrentLevel];
        if (playerStats.GoldAmount < cost)
        {
            Debug.Log("Not enough gold!");
            return;
        }

        // 3. 실제 구매 처리
        playerStats.GoldAmount -= Mathf.RoundToInt(cost);
        powerUp.CurrentLevel++;

        Debug.Log($"Purchased {powerUp.powerUpName} Level {powerUp.CurrentLevel}!");

        // 4. UI 갱신
        currentSelectedButton.UpdateUI(); // 리스트의 아이콘 UI 갱신
        UpdateDetailPanel(powerUp);       // 상세 패널 UI 갱신 (가격 변동 반영)
        UpdateGoldUI();
    }
    public void DeselectOtherButtons()
    {
        foreach (PowerUpButton button in powerUpButtons)
        {
            button.DeSelected();
        }
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
            powerup.UpdateUI();
        }

        UpdateGoldUI();
        
        // 환불 후 현재 선택된 패널 정보도 갱신
        if(currentSelectedButton != null)
        {
            UpdateDetailPanel(currentSelectedButton.powerUp);
        }
    }

    public void UpdateGoldUI()
    {
        if (MyGoldText != null && playerStats != null)
        {
            string GoldText_Format = playerStats.Format(playerStats.GoldAmount);
            MyGoldText.text = $"{GoldText_Format}";
        }
    }
}
