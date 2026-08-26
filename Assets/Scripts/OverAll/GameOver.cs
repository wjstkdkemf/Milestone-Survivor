using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class GameOver : MonoBehaviour
{
    public static GameOver Instance;
    public GameObject Panel;
    public TMP_Text Timer;
    public TMP_Text Kills;
    public TMP_Text TimerText;
    public TMP_Text KillsText;
    public TMP_Text TitleText;
    public bool isOver;
    public int StageToUnlock;
    public GameObject WinnerButton;
    public GameObject LoserButton;
    private void Awake()
    {
        Instance = this;
    }
    private void OnDestroy()
{
    // 내가 현재 활성화된 인스턴스였다면, 파괴될 때 참조를 비워줌
    if (Instance == this)
    {
        Instance = null;
    }
}

    public void GameEnded(bool IsWon = false)
    {
        /*if (isOver)
            return;
        if (Panel != null)
            Panel.SetActive(true);
        if (IsWon)
        {
            if (TitleText != null)
            {
                TitleText.text = "Level Cleared";
                TitleText.color = Color.green;
            }
            if (PlayerStats.Instance.StageCleared <= StageToUnlock)
                PlayerStats.Instance.StageCleared = StageToUnlock;
            WinnerButton.SetActive(true);
            LoserButton.SetActive(false);
        }
        else
        {
            LoserButton.SetActive(true);
            WinnerButton.SetActive(false);
        }

        TimerText.text = "Time: " + Timer.text;
        KillsText.text = "Kills: " + Kills.text;
        isOver = true;
        GameManager.Instance.Pause = true;
        */
        if (isOver)
            return;
        if (Panel != null)
            Panel.SetActive(true);

        if (WinnerButton != null)
            WinnerButton.SetActive(true);
        isOver = true;
        if (GameManager.Instance != null)
            GameManager.Instance.Pause = true;
    }

    public void stageClear(bool IsClear = false)
    {
        if (IsClear)
        {
            // PlayerStats.Instance.SaveStats();
            // UpgradeManager.Instance.SaveUpgrade();
            // UpgradeManager.Instance.SaveCurrentChancesToPersistentData();
            // UpgradeManager.Instance.SaveCurrentPointsToPersistentData();
            if (InfiniteTilemapManager.Instance != null)
                InfiniteTilemapManager.Instance.ClearMap();
            ClearEncount();

            if (GameManager.Instance != null)
                GameManager.Instance.CanSpawn = false;

            return;
            //SceneManager.LoadScene("Map 1");
        }
        else
        {
            
        }
    }
    void ClearEncount()
    {
        GameObject Encounter = GameObject.FindWithTag("EnCount");
        //Debug.Log("체크포인트 1");
        if (Encounter != null)
        {
            EnCounterSystem enCounterSystem = Encounter.GetComponent<EnCounterSystem>();
            if (enCounterSystem != null)
            {
                enCounterSystem.ClearEncount();
            }
        }
    }

    public void MoveInventoryAndLoadScene(string sceneName)
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.StoreInventoryFrom("ClearInventory");

        if (LoadingManager.Instance != null)
            LoadingManager.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}
