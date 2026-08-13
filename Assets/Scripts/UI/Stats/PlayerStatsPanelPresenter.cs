using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class PlayerStatsPanelPresenter : MonoBehaviour
{
    [SerializeField] private StatTableUI statTable;
    [SerializeField] private StatPanelUI statPanel;
    [SerializeField] private bool refreshOnEnable = true;

    private void Awake()
    {
        if (statTable == null)
            statTable = GetComponent<StatTableUI>();

        if (statTable == null && statPanel == null)
            statPanel = GetComponent<StatPanelUI>();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;

        if (refreshOnEnable)
            Refresh();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    public void Refresh()
    {
        if (statTable == null && statPanel == null)
            return;

        var stats = StatEntryFactory.FromPlayerStats(PlayerStats.Instance);
        if (statTable != null)
            statTable.SetStats(stats);
        else
            statPanel.SetStats(stats);
    }

    private void HandleLocaleChanged(Locale locale)
    {
        Refresh();
    }
}
