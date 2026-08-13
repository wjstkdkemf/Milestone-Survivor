using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class TeleportUI : MonoBehaviour
{
    public GameObject teleportSmallButtonPrefab;
    public GameObject teleportBigButtonPrefab;

    public Transform smallButtonContainer;
    public Transform bigButtonContainer;
    public TeleportMapMaker teleportMapMaker;
    public TeleportMapViewport teleportMapViewport;

    public GameObject player;
    public bool IsHome;

    [Header("Button Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.grey;

    private Button SelectbigButton;
    private Button SelectsmallButton;
    private TeleportZoneData currentSelectedGroup;

    private void Start()
    {
        CreateBigTeleportButton();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    private void HandleLocaleChanged(Locale locale)
    {
        CreateBigTeleportButton();

        if (currentSelectedGroup != null)
            CreateTeleportButtons(currentSelectedGroup, FindBigButton(currentSelectedGroup));
    }

    private void CreateBigTeleportButton()
    {
        if (bigButtonContainer == null ||
            teleportBigButtonPrefab == null ||
            TeleportManager.Instance == null ||
            TeleportManager.Instance.database == null)
            return;

        foreach (Transform child in bigButtonContainer)
            Destroy(child.gameObject);

        var zoneGroups = TeleportManager.Instance.database.allZoneGroups;
        if (zoneGroups == null)
            return;

        foreach (TeleportZoneData group in zoneGroups)
        {
            GameObject buttonObj = Instantiate(teleportBigButtonPrefab, bigButtonContainer);
            buttonObj.name = group.zoneName;

            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = group.GetZoneName();

            Image image = buttonObj.GetComponentInChildren<Image>();
            if (image != null && group.zoneSpirte != null)
                image.sprite = group.zoneSpirte;

            TeleportZoneData currentGroup = group;
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => CreateTeleportButtons(currentGroup, button));
        }
    }

    private void CreateTeleportButtons(TeleportZoneData selectedGroup, Button button)
    {
        currentSelectedGroup = selectedGroup;

        if (SelectbigButton != null && SelectbigButton != button)
            SelectbigButton.GetComponent<Image>().color = normalColor;

        SelectbigButton = button;

        if (SelectbigButton != null)
            SelectbigButton.GetComponent<Image>().color = selectedColor;

        ClearPointList();
        SelectsmallButton = null;

        if (teleportMapMaker != null)
            teleportMapMaker.DrawMap(selectedGroup, OnTeleportButtonClick);

        if (teleportMapViewport != null)
            teleportMapViewport.ResetView();
    }

    private void ClearPointList()
    {
        if (smallButtonContainer == null)
            return;

        foreach (Transform child in smallButtonContainer)
            Destroy(child.gameObject);
    }

    public void OnTeleportButtonClick(TeleportData teleportPoint, Button button)
    {
        if (teleportPoint == null)
            return;

        if (SelectsmallButton != null && SelectsmallButton != button)
            SelectsmallButton.GetComponent<Image>().color = normalColor;

        SelectsmallButton = button;

        if (SelectsmallButton != null)
            SelectsmallButton.GetComponent<Image>().color = selectedColor;

        if (IsHome)
        {
            TeleportManager.Instance.startMapName = teleportPoint.GetTargetMapID();
            TeleportManager.Instance.startPointName = teleportPoint.targetSpawnPointID;

            if (teleportMapMaker != null)
                teleportMapMaker.SelectNode(teleportPoint);
        }
        else if (player != null)
        {
            Teleporter teleporter = player.GetComponent<Teleporter>();
            if (teleporter != null)
                teleporter.TeleportTo(teleportPoint.GetTargetMapID(), teleportPoint.targetSpawnPointID);

            gameObject.SetActive(false);
        }
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }

    private Button FindBigButton(TeleportZoneData group)
    {
        if (group == null || bigButtonContainer == null)
            return null;

        foreach (Transform child in bigButtonContainer)
        {
            if (child != null && child.name == group.zoneName)
                return child.GetComponent<Button>();
        }

        return null;
    }
}
