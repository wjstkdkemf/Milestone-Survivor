using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 권장
using System.Collections;
using System.Collections.Generic;

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
    public Image backgroundImage;         // 카드 배경 또는 테두리
    public GameObject legendarySparkleEffect; // 전설 전용 파티클

    [Header("Reward Effects")]
    public GameObject rewardEffectObject; // 보상 연출용 오브젝트
    public float effectDuration = 2.0f;   // 연출 지속 시간
    [Header("Reward UI Settings")]
    public Transform rewardContainer;     // 보상들이 쌓일 부모 오브젝트 (Vertical Layout Group 부착 필수)
    public RewardRowUI rewardRowPrefab;   // 아까 만든 보상 한 줄 프리팹

    // 생성된 보상 UI들을 담아둘 리스트 (재사용 최적화용)
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
        if(currentProgress != null && currentData != null)
            UpdateDisplay();
    }
    private void UpdateDisplay()
    {
        titleText.text = currentData.questName;
        if(descriptionText != null)
            descriptionText.text = currentData.description;
        
        if(currentData.conditions.Count > 0)
        {
            progressText.text = $"{currentProgress.currentCounts[0]} / {currentData.conditions[0].targetAmount}";
        }

        UpdateButtonState();
        DrawRewards(currentData.rewards);
    }
    public void SetupSlot(QuestProgressData progress, QuestDataSO data, bool isMain)
    {
        currentProgress = progress;
        currentData = data;
        titleText.text = data.questName;
        if(descriptionText != null)
            descriptionText.text = data.description;
        //questTypeText.text = isMain ? "<color=#FFD700>메인 퀘스트</color>" : "<color=#ADD8E6>지역 반복 퀘스트</color>";

        // 진행도 텍스트 (다중 조건 중 첫 번째 조건만 대표로 띄우는 예시)
        if (data.conditions.Count > 0)
        {
            progressText.text = $"{progress.currentCounts[0]} / {data.conditions[0].targetAmount}";
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

    private void UpdateButtonState()
    {
        
        if(actionButton == null)
            return;
        // 버튼에 달려있던 기존 함수들 싹 지우기 (버그 방지)
        actionButton.onClick.RemoveAllListeners();

        if (currentProgress.isCompleted)
        {
            // 완료 상태: '보상 받기' 활성화
            //buttonText.text = "보상 받기";
            actionButton.interactable = true;
            actionButton.onClick.AddListener(OnClickClaimReward);
        }
        else
        {
            // 진행 중 상태: 버튼 비활성화 (또는 '포기하기' 로직 연결)
            //buttonText.text = "진행 중";
            actionButton.interactable = false;
        }
    }

    private void OnClickClaimReward()
    {
        StartCoroutine(ClaimRewardSequence());
    }

    private IEnumerator ClaimRewardSequence()
    {
        actionButton.interactable = false;

        // 보상 연출 시작
        if (rewardEffectObject != null)
        {
            rewardEffectObject.SetActive(true);
        }

        // 매니저에게 보상 지급 요청
        QuestManager.Instance.ClaimReward(currentProgress);

        // 연출 시간만큼 대기
        yield return new WaitForSeconds(effectDuration);

        if (rewardEffectObject != null)
        {
            rewardEffectObject.SetActive(false);
        }

        // 새 퀘스트 자동 할당 시도 (서브 퀘스트인 경우)
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
        // 1. 기존에 켜져 있던 보상 UI들을 싹 다 끕니다. (오브젝트 풀링 방식)
        foreach (var row in spawnedRewardRows)
        {
            row.gameObject.SetActive(false);
        }

        // 2. 필요한 만큼 보상 UI를 켜거나 새로 만듭니다.
        for (int i = 0; i < rewards.Count; i++)
        {
            RewardRowUI rowUI;

            // 이미 만들어둔 UI가 있으면 재사용 (Instantiate 최소화)
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
}