
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class TeleportUI : MonoBehaviour
{
    public GameObject teleportButtonPrefab;
    public Transform buttonContainer;
    public GameObject player;

    private List<TeleportPoint> teleportPoints;

    void Start()
    {
        teleportPoints = TeleportManager.Instance.GetTeleportPoints();
        CreateTeleportButtons();
        //gameObject.SetActive(false); // Initially hidden
    }

    void CreateTeleportButtons()
    {
        foreach (TeleportPoint point in teleportPoints)
        {
            GameObject buttonGO = Instantiate(teleportButtonPrefab, buttonContainer);
            Button button = buttonGO.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

            buttonText.text = point.teleportPointName;
            button.onClick.AddListener(() => OnTeleportButtonClick(point.teleportPointName));
        }
    }

    void OnTeleportButtonClick(string teleportPointName)
    {
        if (player != null)
        {
            Teleporter teleporter = player.GetComponent<Teleporter>();
            if (teleporter != null)
            {
                teleporter.TeleportTo(teleportPointName);
            }
        }
        gameObject.SetActive(false); // Hide UI after teleporting
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}
