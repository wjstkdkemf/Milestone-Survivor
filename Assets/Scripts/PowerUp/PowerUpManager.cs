using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [Header("Tier Data")]
    public List<PowerUpTier> powerUpTiers; // 티어별 데이터 리스트
    private int currentTierIndex = 0;      // 현재 보고 있는 티어

    [Header("UI References")]
    public Transform buttonContainer;      // ScrollView의 Content (버튼들의 부모)
    public GameObject globalLockPanel;     // 잠겨있을 때 화면을 덮을 패널
    public Button leftButton;
    public Button rightButton;
    public TMP_Text tierTitleText;

    // 미리 생성해둔 버튼 풀
    private List<PowerUpButton> buttonPool = new List<PowerUpButton>();

    [Header("Details Panel UI")]
    public GameObject panel;
    public TMP_Text MyGoldText;
    public TMP_Text nameText;
    public TMP_Text currentLevelText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public GameObject BuyButtons;
    public Image PowerUpIcon;
    public PlayerStats playerStats;

    private PowerUpButton currentSelectedButton;
    public bool InGame;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (InGame) return;

        playerStats = FindAnyObjectByType<PlayerStats>();

        InitializeButtonPool();

        UpdateTierView();
        UpdateGoldUI();
    }

    void InitializeButtonPool()
    {
        if(buttonContainer != null)
            buttonPool = buttonContainer.GetComponentsInChildren<PowerUpButton>(true).ToList();

        if (buttonPool.Count == 0)
        {
            Debug.LogWarning("PowerUpManager: ScrollView Content 안에 PowerUpButton이 하나도 없습니다!");
        }
    }
    
    public void OnClickLeftBtn()
    {
        if (currentTierIndex > 0)
        {
            currentTierIndex--;
            UpdateTierView();
            //if(panel != null) panel.SetActive(false); 
        }
    }

    public void OnClickRightBtn()
    {
        if (currentTierIndex < powerUpTiers.Count - 1)
        {
            currentTierIndex++;
            UpdateTierView();
            //if(panel != null) panel.SetActive(false);
        }
    }

    public void UpdateTierView()
    {
        PowerUpTier currentTier = powerUpTiers[currentTierIndex];

        if (tierTitleText != null)
            tierTitleText.text = currentTier.tierName;

        int dataCount = currentTier.tierPowerUps.Count;
        
        for (int i = 0; i < buttonPool.Count; i++)
        {
            if (i < dataCount)
            {
                PowerUpButton btn = buttonPool[i];
                btn.gameObject.SetActive(true);
                btn.powerUp = currentTier.tierPowerUps[i]; // 데이터 교체
                btn.Initialize(this); // UI 새로고침
                
                // 버튼 선택 상태 초기화
                btn.DeSelected(); 
            }
            else
            {
                buttonPool[i].gameObject.SetActive(false);
            }
        }

        CheckLockStatus();

        if (leftButton != null) leftButton.interactable = (currentTierIndex > 0);
        if (rightButton != null) rightButton.interactable = (currentTierIndex < powerUpTiers.Count - 1);
    }

    void CheckLockStatus()
    {
        bool isLocked = IsCurrentTierLocked();

        if (globalLockPanel != null)
        {
            globalLockPanel.SetActive(isLocked);
        }
    }

    public bool IsCurrentTierLocked()
    {
        if (currentTierIndex == 0) return false;
        
        var prevTier = powerUpTiers[currentTierIndex - 1];
        return !prevTier.IsAllMaxed();
    }


    public void Purchase()
    {
        if (currentSelectedButton == null) return;
        
        if (IsCurrentTierLocked())
        {
            Debug.Log("이전 단계 강화를 모두 완료해야 합니다!");
            return;
        }

        PowerUpScriptableObject powerUp = currentSelectedButton.powerUp;

        if (powerUp.CurrentLevel >= powerUp.upgradeValues.Length)
        {
            Debug.Log("Already Max Level!");
            return;
        }

        float cost = powerUp.costPerLevel[powerUp.CurrentLevel];
        if (playerStats.GoldAmount < cost)
        {
            Debug.Log("Not enough gold!");
            return;
        }

        playerStats.GoldAmount -= Mathf.RoundToInt(cost);
        powerUp.CurrentLevel++;

        currentSelectedButton.UpdateUI();
        UpdateDetailPanel(powerUp);
        UpdateGoldUI();
        LoadScreenManager.Instance.ConfirmSelectionSave();

        CheckLockStatus(); 
    }

    public void RefundPowerUp()
    {
        foreach (var tier in powerUpTiers)
        {
            foreach (var powerUp in tier.tierPowerUps)
            {
                if (powerUp.CurrentLevel > 0)
                {
                    float amount = 0;
                    for (int i = 0; i < powerUp.CurrentLevel; i++)
                    {
                        amount += powerUp.costPerLevel[i];
                    }
                    PlayerStats.Instance.GoldAmount += Mathf.RoundToInt(amount);
                    
                    powerUp.CurrentLevel = 0;
                }
            }
        }

        UpdateGoldUI();
        
        foreach (var btn in buttonPool)
        {
            if(btn.gameObject.activeSelf) 
                btn.UpdateUI();
        }

        if (currentSelectedButton != null && panel != null)
        {
            UpdateDetailPanel(currentSelectedButton.powerUp);
        }
        
        CheckLockStatus();
    }


    public void SetInfo(PowerUpButton button)
    {
        currentSelectedButton = button;
        if (panel != null) UpdateDetailPanel(button.powerUp);
    }

    public void DeselectOtherButtons()
    {
        foreach (var btn in buttonPool)
        {
            if(btn.gameObject.activeSelf)
                btn.DeSelected();
        }
    }

    private void UpdateDetailPanel(PowerUpScriptableObject info)
    {
        panel.SetActive(true);
        nameText.text = info.powerUpName;
        descriptionText.text = info.description;
        currentLevelText.text = "+ " + info.CurrentLevel.ToString();
        PowerUpIcon.sprite = info.IconSprite;

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

    public void UpdateGoldUI()
    {
        if (MyGoldText != null && playerStats != null)
        {
            MyGoldText.text = playerStats.Format(playerStats.GoldAmount);
        }
    }

    // --- Save / Load ---

    public PowerUpSaveData GetSaveData()
    {
        var powerUpLevels = new Dictionary<PowerUpType, int>();
        foreach (var tier in powerUpTiers)
        {
            foreach (var powerUp in tier.tierPowerUps)
            {
                powerUpLevels[powerUp.powerUpType] = powerUp.CurrentLevel;
            }
        }
        return new PowerUpSaveData(powerUpLevels);
    }

    public void LoadData(PowerUpSaveData data)
    {
        if (data == null)
        {
            foreach (var tier in powerUpTiers)
            {
                foreach (var powerUp in tier.tierPowerUps)
                {
                    powerUp.CurrentLevel = 0;
                }
            }
        }
        else
        {
            var powerUpLevels = data.ToDictionary();
            foreach (var tier in powerUpTiers)
            {
                foreach (var powerUp in tier.tierPowerUps)
                {
                    if (powerUpLevels.ContainsKey(powerUp.powerUpType))
                    {
                        powerUp.CurrentLevel = powerUpLevels[powerUp.powerUpType];
                    }
                }
            }
        }

        UpdateTierView();
        UpdateGoldUI();
    }
}