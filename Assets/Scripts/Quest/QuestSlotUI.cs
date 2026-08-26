using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 권장
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class QuestSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI questTypeText; // "메인 퀘스트" or "반복 퀘스트"
    public TextMeshProUGUI progressText;  // "10 / 50"
    public bool Ingame = false;

    public Button actionButton;           // 수락 / 보상받기 버튼
    public TextMeshProUGUI buttonText;

    [Header("Rarity Effects")]
    //public Image backgroundImage;         // 카드 배경 또는 테두리
    [SerializeField] private Image border;
    private Coroutine highlightRoutine;
    public GameObject legendarySparkleEffect; // 전설 전용 파티클

    [Header("Reward Effects")]
    public GameObject rewardEffectObject; // 보상 연출용 오브젝트
    public float effectDuration = 2.0f;   // 연출 지속 시간
    [Header("Reward UI Settings")]
    public Transform rewardContainer;     // 보상들이 쌓일 부모 오브젝트
    public RewardRowUI rewardRowPrefab;   // 아까 만든 보상 한 줄 프리팹
    public TextMeshProUGUI RewardNormalString;
    [SerializeField] private LocalizedString RewardString;


    [Header("Localization")]
    [SerializeField] private LocalizedString claimRewardString;

    // 생성된 보상 UI들을 담아둘 리스트
    private List<RewardRowUI> spawnedRewardRows = new List<RewardRowUI>();

    private QuestProgressData currentProgress;
    private QuestDataSO currentData;
    private QuestUIManager uiManager;

    private void Awake()
    {
        uiManager = GetComponentInParent<QuestUIManager>();
    }
    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;

        if(currentProgress != null && currentData != null)
            UpdateDisplay();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    private void HandleLocaleChanged(Locale locale)
    {
        if(currentProgress != null && currentData != null)
            UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if(!CheckNullComponent(currentProgress,currentData))
            return;

        titleText.text = currentData.GetLocalizedQuestName();
        if(descriptionText != null)
            descriptionText.text = currentData.GetLocalizedDescription();

        if(RewardNormalString != null)
            RewardNormalString.text = RewardString.GetLocalizedString();
        
        if(currentData.conditions.Count > 0 && currentProgress.currentCounts.Count > 0)
        {
            progressText.text = $"{currentProgress.currentCounts[0]} / {currentData.conditions[0].targetAmount}";
        }

        UpdateButtonState();
        DrawRewards(currentData.rewards);
    }
    public void SetupSlot(QuestProgressData progress, QuestDataSO data, bool isMain)
    {
        if(!CheckNullComponent(progress, data))
            return;

        currentProgress = progress;
        currentData = data;
        titleText.text = data.GetLocalizedQuestName();

        if(RewardNormalString != null)
            RewardNormalString.text = RewardString.GetLocalizedString();

        if(descriptionText != null)
            descriptionText.text = data.GetLocalizedDescription();
        //questTypeText.text = isMain ? "<color=#FFD700>메인 퀘스트</color>" : "<color=#ADD8E6>지역 반복 퀘스트</color>";

        // 진행도 텍스트 (다중 조건 중 첫 번째 조건만 대표로 띄우는 예시)
        if (currentData.conditions.Count > 0 && currentProgress.currentCounts.Count > 0)
        {
            progressText.text = $"{currentProgress.currentCounts[0]} / {currentData.conditions[0].targetAmount}";
        }
        else
        {
            progressText.text = ""; // 조건이 없는 퀘스트라면 텍스트를 비워줍니다.
        }

        ApplyRarityEffect(data.rarity);

        UpdateButtonState();

        DrawRewards(data.rewards);
    }
    private void ApplyRarityEffect(QuestRarity rarity)
    {
        if (legendarySparkleEffect != null) 
            legendarySparkleEffect.SetActive(false);

        /*switch (rarity)
        {
            case QuestRarity.Common:
                backgroundImage.color = Color.white;
                break;
            case QuestRarity.Rare:
                backgroundImage.color = new Color(0.2f, 0.5f, 1f); // 파란색 테두리
                break;
            case QuestRarity.Epic:
                backgroundImage.color = new Color(0.7f, 0.2f, 0.9f); // 보라색 테두리
                break;
            case QuestRarity.Legendary:
                backgroundImage.color = new Color(1f, 0.8f, 0.2f); // 황금색 테두리
                if (legendarySparkleEffect != null) 
                    legendarySparkleEffect.SetActive(true); // 이펙트 켜기
                break;
        }
        */
    }
    void StartHighlight()
    {
        if (highlightRoutine != null) return;
        highlightRoutine = StartCoroutine(HighlightRoutine());
    }

    void StopHighlight()
    {
        if (highlightRoutine != null)
        {
            StopCoroutine(highlightRoutine);
            highlightRoutine = null;
        }
        if(border != null)
            border.color = Color.white;
    }
    IEnumerator HighlightRoutine()
    {
        Color baseColor = Color.white;
        Color glowColor = new Color(1f, 0.85f, 0.2f);

        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * 2f;

            float lerp = (Mathf.Sin(t) + 1f) * 0.5f;

            border.color = Color.Lerp(baseColor, glowColor, lerp);

            yield return null;
        }
    }

    private void UpdateButtonState()
    {
        
        if(actionButton == null)
            return;
        // 버튼에 달려있던 기존 함수들 싹 지우기
        actionButton.onClick.RemoveAllListeners();

        bool isCompleted = currentProgress.isCompleted;
        bool isClaimed = currentProgress.isClaimed;

        if (isCompleted && !isClaimed && !Ingame)
        {
            // 보상 받기 가능 상태
            string claimRewardText = GetClaimRewardText();
            progressText.text = claimRewardText;
            if(buttonText != null)
                buttonText.text = claimRewardText;
            actionButton.interactable = true;
            actionButton.onClick.AddListener(OnClickClaimReward);

            StartHighlight(); // 반짝 시작
        }
        else
        {
            actionButton.interactable = false;
            StopHighlight(); // 반짝 중지
        }
    }

    private void OnClickClaimReward()
    {
        StartCoroutine(ClaimRewardSequence());
    }

    private IEnumerator ClaimRewardSequence()
    {
        actionButton.interactable = false;

        StopHighlight();

        currentProgress.isClaimed = true;

        QuestManager.Instance.ClaimReward(currentProgress);

        // 보상 연출 시작
        if (rewardEffectObject != null)
        {
            rewardEffectObject.SetActive(true);
        }
        // 연출 시간만큼 대기
        yield return new WaitForSeconds(effectDuration);

        if (rewardEffectObject != null)
        {
            rewardEffectObject.SetActive(false);
        }

        // 새 퀘스트 자동 할당 시도
        QuestManager.Instance.FillSubQuests();

        // UI 갱신
        if (uiManager != null)
        {
            uiManager.RefreshAllQuestUI();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    private void DrawRewards(List<QuestReward> rewards)
    {
        if(Ingame)
            return;

        if(rewardRowPrefab == null
        || rewardContainer == null
        || rewards == null)
            return;
        // 기존에 켜져 있던 보상 UI들을 싹 다 끕니다.
        foreach (var row in spawnedRewardRows)
        {
            row.gameObject.SetActive(false);
        }

        // 필요한 만큼 보상 UI를 켜거나 새로 만듭니다.
        for (int i = 0; i < rewards.Count; i++)
        {
            RewardRowUI rowUI;

            // 이미 만들어둔 UI가 있으면 재사용
            if (i < spawnedRewardRows.Count)
            {
                rowUI = spawnedRewardRows[i];
            }
            else // 모자라면 새로 생성
            {
                rowUI = Instantiate(rewardRowPrefab, rewardContainer);
                spawnedRewardRows.Add(rowUI);
            }

            rowUI.gameObject.SetActive(true);
            rowUI.Setup(rewards[i]); // 데이터 주입!
        }
    }

    private string GetClaimRewardText()
    {
        return QuestLocalization.Get(
            claimRewardString,
            QuestLocalization.ClaimRewardKey,
            "보상 받기"
        );
    }

    private bool CheckNullComponent(QuestProgressData progress, QuestDataSO data)
    {
        if(titleText == null
        || progressText == null
        || border == null
        || QuestManager.Instance == null
        || progress == null
        || data == null)
        {
            DevLog.Log("퀘스트 슬롯 UI 연결 체크");
            return false;
        }
        return true;
    }
}
