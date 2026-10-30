using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeJobRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text jobNameText;
    [SerializeField] private Transform requirementsRoot;
    [SerializeField] private Transform rewardsRoot;
    [SerializeField] private UpgradeJobSkillUI skillPrefab;
    private readonly List<UpgradeJobSkillUI> spawnedItems = new();

    public void SetInfo(
        JobDataSO job,
        List<UpgradeScriptableObject> masterDeck)
    {
        foreach (var item in spawnedItems)
        {
            if (item == null) continue;
            item.gameObject.SetActive(false);
            Destroy(item.gameObject);
        }
        spawnedItems.Clear();

        jobNameText.text = job != null ? job.jobName : "";
        if (job == null) return;

        if (job.requirements != null)
        {
            foreach (var requirement in job.requirements)
            {
                var required = requirement.requiredUpgrade;
                if (required == null) continue;

                var owned = masterDeck?.Find(
                    card => card != null && card.name == required.name);
                int points = owned != null ? owned.Points : 0;
                CreateItem(requirementsRoot).SetRequirement(
                    required, points, requirement.requiredLevel);
            }
        }

        if (job.bonusUpgrades != null)
        {
            foreach (var reward in job.bonusUpgrades)
            {
                if (reward == null) continue;
                CreateItem(rewardsRoot).SetReward(reward);
            }
        }
    }

    private UpgradeJobSkillUI CreateItem(Transform parent)
    {
        var item = Instantiate(skillPrefab, parent);
        item.gameObject.SetActive(true);
        spawnedItems.Add(item);
        return item;
    }
}