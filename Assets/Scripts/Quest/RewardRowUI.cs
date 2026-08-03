using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardRowUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI rewardText;

    public void Setup(QuestReward reward)
    {
        if (reward.rewardIcon != null)
        {
            iconImage.sprite = reward.rewardIcon;
        }

        switch (reward.rewardType)
        {
            case RewardType.Gold:
                rewardText.text = $"{reward.amount} G";
                //PlayerStats.Instance.AddCoin(reward.amount);
                break;
            case RewardType.Item:
                if (reward.lootTable != null)
                {
                    rewardText.text = reward.lootTable.GetLocalizedDisplayName();
                }
                else
                {
                    rewardText.text = "데이터 누락됨";
                    Debug.LogError("보상 아이템 데이터가 없습니다!");
                }
                break;
        }

        //LoadScreenManager.Instance.ConfirmSelectionSave();
    }
}
