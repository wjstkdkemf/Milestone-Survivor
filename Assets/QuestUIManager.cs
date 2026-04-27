using UnityEngine;
using System.Collections.Generic;

public class QuestUIManager : MonoBehaviour
{
    [Header("UI Slots")]
    public QuestSlotUI mainQuestSlot;        // 메인 퀘스트 슬롯 (1개)
    public List<QuestSlotUI> subQuestSlots;  // 서브 퀘스트 슬롯들 (2개)

    private void Start()
    {
        // 게임 시작 시 UI를 최신화합니다.
        RefreshAllQuestUI();
    }

    // 💡 이 함수가 핵심입니다! 데이터와 UI를 연결해 줍니다.
    public void RefreshAllQuestUI()
    {
        // 1. 메인 퀘스트 데이터 연결
        var mainData = QuestManager.Instance.currentMainQuest;
        if (mainData != null)
        {
            var so = QuestManager.Instance.GetQuestSO(mainData.questID);
            mainQuestSlot.gameObject.SetActive(true);
            mainQuestSlot.SetupSlot(mainData, so, true); // 여기서 SetupSlot 호출!
        }
        else
        {
            mainQuestSlot.gameObject.SetActive(false);
        }

        // 2. 서브 퀘스트 데이터 연결
        var subDataList = QuestManager.Instance.currentSubQuests;
        for (int i = 0; i < subQuestSlots.Count; i++)
        {
            if (i < subDataList.Count)
            {
                var so = QuestManager.Instance.GetQuestSO(subDataList[i].questID);
                subQuestSlots[i].gameObject.SetActive(true);
                subQuestSlots[i].SetupSlot(subDataList[i], so, false); // 여기서 SetupSlot 호출!
            }
            else
            {
                subQuestSlots[i].gameObject.SetActive(false);
            }
        }
    }
}