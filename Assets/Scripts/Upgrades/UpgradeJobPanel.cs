using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeJobPanel : MonoBehaviour
{
    [SerializeField] private UpgradeJobRowUI rowUI;
    [SerializeField] private GameObject navigationRoot;
    [SerializeField] private TMP_Text pageText;

    private readonly List<JobDataSO> jobs = new();
    private UpgradeManager manager;
    private int selectedIndex;

    public void SetInfo(
        UpgradeScriptableObject upgrade,
        UpgradeManager upgradeManager)
    {
        SetVisible(false);
        manager = upgradeManager;
        selectedIndex = 0;
        jobs.Clear();

        if (manager != null)
            manager.CollectRelatedJobs(upgrade, jobs);

        RefreshPage();
    }

    public void ChangePage(int direction)
    {
        if (jobs.Count <= 1 || manager == null) return;

        selectedIndex = (selectedIndex + direction % jobs.Count + jobs.Count) % jobs.Count;
        RefreshPage();
    }

    private void RefreshPage()
    {
        bool hasJobs = manager != null && jobs.Count > 0;
        rowUI.gameObject.SetActive(hasJobs);
        navigationRoot.SetActive(hasJobs && jobs.Count > 1);
        pageText.text = hasJobs ? $"{selectedIndex + 1}/{jobs.Count}" : "";

        if (hasJobs)
            rowUI.SetInfo(jobs[selectedIndex], manager.MasterDeck);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible && manager != null && jobs.Count > 0);
    }
}