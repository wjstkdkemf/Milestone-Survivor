using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 권장

public class QuestSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI questTypeText; // "메인 퀘스트" or "반복 퀘스트"
    public TextMeshProUGUI progressText;  // "10 / 50"

    public Button actionButton;           // 수락 / 보상받기 버튼
    public TextMeshProUGUI buttonText;

    [Header("Rarity Effects")]
    public Image backgroundImage;         // 카드 배경 또는 테두리
    public GameObject legendarySparkleEffect; // 전설 전용 파티클

    private QuestProgressData currentProgress;
    private QuestDataSO currentData;

    public void SetupSlot(QuestProgressData progress, QuestDataSO data, bool isMain)
    {
        currentProgress = progress;
        currentData = data;

        titleText.text = data.questName;
        descriptionText.text = data.description;
        questTypeText.text = isMain ? "<color=#FFD700>메인 퀘스트</color>" : "<color=#ADD8E6>지역 반복 퀘스트</color>";

        // 진행도 텍스트 (다중 조건 중 첫 번째 조건만 대표로 띄우는 예시)
        if (data.conditions.Count > 0)
        {
            progressText.text = $"{progress.currentCounts[0]} / {data.conditions[0].targetAmount}";
        }

        ApplyRarityEffect(data.rarity);

        UpdateButtonState();
    }
    private void ApplyRarityEffect(QuestRarity rarity)
    {
        if (legendarySparkleEffect != null) 
            legendarySparkleEffect.SetActive(false);

        switch (rarity)
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
    }

    private void UpdateButtonState()
    {
        // 버튼에 달려있던 기존 함수들 싹 지우기 (버그 방지)
        actionButton.onClick.RemoveAllListeners();

        if (currentProgress.isCompleted)
        {
            // 완료 상태: '보상 받기' 활성화
            buttonText.text = "보상 받기";
            actionButton.interactable = true;
            actionButton.onClick.AddListener(OnClickClaimReward);
        }
        else
        {
            // 진행 중 상태: 버튼 비활성화 (또는 '포기하기' 로직 연결)
            buttonText.text = "진행 중";
            actionButton.interactable = false;
        }
    }

    private void OnClickClaimReward()
    {
        // 매니저에게 보상 지급 요청
        QuestManager.Instance.ClaimReward(currentProgress);
        
        // 보상 받은 후 UI 갱신 (슬롯 비우기 처리 등)
        gameObject.SetActive(false); 
    }
}