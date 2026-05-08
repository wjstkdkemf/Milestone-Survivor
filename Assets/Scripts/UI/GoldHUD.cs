using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    private PlayerStats playerStats;

    private bool isSubscribed;

    private void Start()
    {
        TryInitialize();
    }

    private void OnEnable()
    {
        TryInitialize();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TryInitialize()
    {
        if (playerStats == null)
        {
            playerStats = PlayerStats.Instance;
        }

        if (playerStats == null)
            return;

        Subscribe();

        UpdateGoldUI(playerStats.GoldAmount);
    }

    private void Subscribe()
    {
        if (isSubscribed)
            return;

        playerStats.OnGoldChanged += UpdateGoldUI;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (playerStats == null || !isSubscribed)
            return;

        playerStats.OnGoldChanged -= UpdateGoldUI;
        isSubscribed = false;
    }

    private void UpdateGoldUI(long gold)
    {
        if (goldText == null || playerStats == null)
            return;

        goldText.text = playerStats.Format(gold);
    }
}
