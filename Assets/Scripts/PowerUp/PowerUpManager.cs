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
        // 내가 현재 활성화된 인스턴스였다면, 파괴될 때 참조를 비워줌
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (InGame) return;

        playerStats = FindAnyObjectByType<PlayerStats>();

        // 1. 버튼 풀 초기화 (Content 자식에 있는 버튼들을 긁어옴)
        InitializeButtonPool();

        // 2. 초기 UI 설정 (0페이지)
        UpdateTierView();
        UpdateGoldUI();
    }

    // --- 초기화: 미리 배치된 버튼들을 풀에 등록 ---
    void InitializeButtonPool()
    {
        // buttonContainer 아래의 모든 PowerUpButton 컴포넌트를 가져옴
        // (IncludeInactive = true로 하여 꺼져있는 것도 가져옴)
        if(buttonContainer != null)
            buttonPool = buttonContainer.GetComponentsInChildren<PowerUpButton>(true).ToList();

        if (buttonPool.Count == 0)
        {
            Debug.LogWarning("PowerUpManager: ScrollView Content 안에 PowerUpButton이 하나도 없습니다!");
        }
    }

    // --- 페이지 전환 ---
    
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

    // [핵심] 현재 티어에 맞춰 버튼 재활용 및 UI 갱신
    public void UpdateTierView()
    {
        // 1. 현재 티어 데이터 가져오기
        PowerUpTier currentTier = powerUpTiers[currentTierIndex];

        // 2. 타이틀 갱신
        if (tierTitleText != null)
            tierTitleText.text = currentTier.tierName;

        // 3. 버튼 풀링 로직 (데이터 바인딩)
        int dataCount = currentTier.tierPowerUps.Count;
        
        for (int i = 0; i < buttonPool.Count; i++)
        {
            if (i < dataCount)
            {
                // 사용할 버튼: 켜고 데이터 연결
                PowerUpButton btn = buttonPool[i];
                btn.gameObject.SetActive(true);
                btn.powerUp = currentTier.tierPowerUps[i]; // 데이터 교체
                btn.Initialize(this); // UI 새로고침 (이름, 레벨 등)
                
                // 버튼 선택 상태 초기화 (페이지 넘길 때 선택 해제)
                btn.DeSelected(); 
            }
            else
            {
                // 남는 버튼: 끄기
                buttonPool[i].gameObject.SetActive(false);
            }
        }

        // 4. 잠금 상태 체크 (이전 티어 완료 여부)
        CheckLockStatus();

        // 5. 네비게이션 버튼 갱신
        if (leftButton != null) leftButton.interactable = (currentTierIndex > 0);
        if (rightButton != null) rightButton.interactable = (currentTierIndex < powerUpTiers.Count - 1);
    }

    // 잠금 상태 확인 및 처리
    void CheckLockStatus()
    {
        bool isLocked = IsCurrentTierLocked();

        // 전역 잠금 패널 제어
        if (globalLockPanel != null)
        {
            globalLockPanel.SetActive(isLocked);
            // 만약 패널에 텍스트가 있다면 "이전 단계 훈련을 완료하세요" 등으로 변경 가능
        }

        // 잠겨있으면 버튼들 상호작용 막기 (패널이 덮으면 굳이 안 해도 되지만 안전장치)
        /*
        foreach (var btn in buttonPool)
        {
            if (btn.gameObject.activeSelf)
                btn.GetComponent<Button>().interactable = !isLocked;
        }
        */
    }

    public bool IsCurrentTierLocked()
    {
        if (currentTierIndex == 0) return false;
        
        // 이전 티어 확인
        var prevTier = powerUpTiers[currentTierIndex - 1];
        return !prevTier.IsAllMaxed();
    }

    // --- 구매 로직 ---

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

        // UI 갱신
        currentSelectedButton.UpdateUI();
        UpdateDetailPanel(powerUp);
        UpdateGoldUI();
        LoadScreenManager.Instance.ConfirmSelectionSave();

        // 구매로 인해 다음 티어 해금 조건이 바뀔 수 있으므로 체크 (필요시)
        CheckLockStatus(); 
    }

    // --- 환불 로직 (데이터 기반) ---
    // 버튼 인스턴스는 현재 페이지만 보여주므로, 전체 환불을 위해선 데이터를 순회해야 함
    public void RefundPowerUp()
    {
        // 모든 티어 데이터 순회
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
                    
                    // 데이터 초기화
                    powerUp.CurrentLevel = 0;
                }
            }
        }

        UpdateGoldUI();
        
        // 현재 보고 있는 페이지의 버튼들 UI 갱신 (데이터가 0이 되었으니)
        foreach (var btn in buttonPool)
        {
            if(btn.gameObject.activeSelf) 
                btn.UpdateUI();
        }

        // 상세 패널 갱신
        if (currentSelectedButton != null && panel != null)
        {
            UpdateDetailPanel(currentSelectedButton.powerUp);
        }
        
        CheckLockStatus();
    }

    // --- UI 및 기타 함수들 ---

    public void SetInfo(PowerUpButton button)
    {
        currentSelectedButton = button;
        if (panel != null) UpdateDetailPanel(button.powerUp);
    }

    public void DeselectOtherButtons()
    {
        // 현재 활성화된 버튼 풀만 돌면 됨
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

        // 데이터 로드 후 현재 뷰 갱신
        UpdateTierView();
        UpdateGoldUI();
    }
}