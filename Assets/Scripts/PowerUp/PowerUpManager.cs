using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

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
    [SerializeField]private GameObject panel;
    [SerializeField]private TMP_Text MyGoldText;
    [SerializeField]private TMP_Text nameText;
    [SerializeField]private TMP_Text currentLevelText;
    [SerializeField]private TMP_Text costText;
    [SerializeField]private TMP_Text descriptionText;
    [SerializeField]private TMP_Text Statname;
    [SerializeField]private TMP_Text beforeStat;
    [SerializeField]private TMP_Text afterStat;
    [SerializeField]private GameObject BuyButtons;
    [SerializeField]private Image PowerUpIcon;
    [SerializeField]private GameObject statArrow;
    [SerializeField]private PlayerStats playerStats;

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
        InitializeUpgradePanel();

        UpdateTierView();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    private void HandleLocaleChanged(Locale locale)
    {
        if (InGame) return;

        UpdateTierView();

        if (currentSelectedButton != null && panel != null && panel.activeSelf)
            UpdateDetailPanel(currentSelectedButton.powerUp);
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
    void InitializeUpgradePanel()
    {
        nameText.text = "---";
        currentLevelText.text = "--";
        costText.text = "---";
        descriptionText.text = "---";
        Statname.text = "";
        beforeStat.text = "";
        afterStat.text = "";
        statArrow.SetActive(false);
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
            tierTitleText.text = currentTier.GetLocalizedTierName();

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

        long cost = powerUp.costPerLevel[powerUp.CurrentLevel];
        if (!playerStats.TrySpendGold(cost))
        {
            Debug.Log("Not enough gold!");
            return;
        }

        powerUp.CurrentLevel++;

        currentSelectedButton.UpdateUI();
        UpdateDetailPanel(powerUp);
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
                    long amount = 0;
                    for (int i = 0; i < powerUp.CurrentLevel; i++)
                    {
                        amount += powerUp.costPerLevel[i];
                    }
                    PlayerStats.Instance.AddGold(amount);
                    
                    powerUp.CurrentLevel = 0;
                }
            }
        }

        
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
        nameText.text = info.GetLocalizedName();
        descriptionText.text = info.GetLocalizedDescription();
        currentLevelText.text = "+ " + info.CurrentLevel.ToString();
        PowerUpIcon.sprite = info.IconSprite;

        bool isMaxLevel = info.CurrentLevel >= info.upgradeValues.Length;

        if (!isMaxLevel)
        {
            costText.text = info.costPerLevel[info.CurrentLevel].ToString();
            //Statname.text = info.powerUpType.ToString();
            beforeStat.text = info.upgradeValues[info.CurrentLevel].ToString();
            afterStat.text = info.upgradeValues[info.CurrentLevel + 1].ToString();
            statArrow.SetActive(true);
            BuyButtons.SetActive(true);
        }
        else
        {
            costText.text = "MAX";
            //Statname.text = info.powerUpType.ToString();
            beforeStat.text = info.upgradeValues[info.CurrentLevel].ToString();
            statArrow.SetActive(false);
            BuyButtons.SetActive(false);
        }
    }


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
        ResetAllPowerUpLevels();

        if (data != null)
        {
            var powerUpLevels = data.ToDictionary();
            foreach (var tier in powerUpTiers)
            {
                foreach (var powerUp in tier.tierPowerUps)
                {
                    if (powerUpLevels.ContainsKey(powerUp.powerUpType))
                    {
                        powerUp.CurrentLevel = Mathf.Clamp(powerUpLevels[powerUp.powerUpType], 0, powerUp.upgradeValues.Length);
                    }
                }
            }
        }

        UpdateTierView();
    }

    private void ResetAllPowerUpLevels()
    {
        foreach (var tier in powerUpTiers)
        {
            foreach (var powerUp in tier.tierPowerUps)
            {
                powerUp.CurrentLevel = 0;
            }
        }
    }
}
