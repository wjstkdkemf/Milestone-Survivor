using UnityEngine;
using System.Collections.Generic;

public class QuestUIManager : MonoBehaviour
{
    [Header("UI Slots")]
    public QuestSlotUI mainQuestSlot;        // 메인 퀘스트 슬롯 (1개)
    public List<QuestSlotUI> subQuestSlots;  // 서브 퀘스트 슬롯들 (2개)

    private void Start()
    {
        RefreshAllQuestUI();
    }

    public void RefreshAllQuestUI()
    {
        // 서브 퀘스트가 비어있으면 자동으로 채우기 시도
        if (QuestManager.Instance.currentSubQuests.Count < subQuestSlots.Count)
        {
            QuestManager.Instance.FillSubQuests();
        }

        var mainData = QuestManager.Instance.currentMainQuest;
        if (mainData != null)
        {
            var so = QuestManager.Instance.GetQuestSO(mainData.questID);
            mainQuestSlot.gameObject.SetActive(true);
            Debug.Log(mainData.questID);
            mainQuestSlot.SetupSlot(mainData, so, true);
        }
        else
        {
            mainQuestSlot.gameObject.SetActive(false);
        }

        var subDataList = QuestManager.Instance.currentSubQuests;
        for (int i = 0; i < subQuestSlots.Count; i++)
        {
            if (i < subDataList.Count)
            {
                var so = QuestManager.Instance.GetQuestSO(subDataList[i].questID);
                subQuestSlots[i].gameObject.SetActive(true);
                subQuestSlots[i].SetupSlot(subDataList[i], so, false);
            }
            else
            {
                subQuestSlots[i].gameObject.SetActive(false);
            }
        }
    }
}