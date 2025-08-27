
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelection : MonoBehaviour
{

    public List<StageButtonUi> stageButtons = new List<StageButtonUi>();
    public GameObject panle;
    public TMP_Text StageStats;
    public GameObject StartButtons;
    public Image icon;
    public StageButtonUi stageSelectionButton;
    private void Start()
    {
        foreach (StageButtonUi button in stageButtons)
        {
            button.Initialize(this);
        }
        UnlockStages();
        Invoke("delayedStart", .2f);

    }
    void delayedStart()
    {
        stageButtons[0].Selected();
    }
    public void SetInfo(StageScriptableObject info, StageButtonUi button)
    {
        panle.SetActive(true);
        stageSelectionButton = button;
        StageStats.text = info.GetStatsAsString();

        StartButtons.SetActive(info.IsUnlocked);
    }
    public void DeselectOtherButtons()
    {
        foreach (StageButtonUi button in stageButtons)
        {
            button.DeSelected();
        }
    }
    public void StartThegame()
    {
        //SceneManager.LoadScene(4);//stageSelectionButton.stageInfo.SceneIndex

        GameManager.Instance.SelectCharacter(PlayerStats.Instance.CharacterID);
    }
    public void UnlockStages()
    {
        foreach (StageButtonUi button in stageButtons)
        {
            button.stageInfo.IsUnlocked = false;
        }

        for (int i = 0; i <= PlayerStats.Instance.StageCleared+1; i++)
        {
            stageButtons[i].stageInfo.IsUnlocked = true;
        }
    }
    public void ResetStages()
    {
        foreach (StageButtonUi button in stageButtons)
        {
            button.stageInfo.IsUnlocked = false;
        }
        stageButtons[0].stageInfo.IsUnlocked = true;
        PlayerStats.Instance.StageCleared = 0;
        delayedStart();
    }
}
