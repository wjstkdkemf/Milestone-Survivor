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
                break;
            case RewardType.Item:
                rewardText.text = reward.itemName; // "초보자의 검" 등
                break;
        }
    }
}