
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool StopMoveing;
    private readonly HashSet<MonoBehaviour> pauseRequesters = new();
    public bool Pause { get; private set; }
    public bool CanProcessCombat => !Pause;

    public TMP_Text TextKill;
    public TMP_Text Player_Level_Count;
    public int NumberOfKills;
    public int activeEnemies = 0;
    private int lastKillCount = -1;
    private int lastLevel = -1;
    public bool AllKill = false;
    public bool Heal = false;
    public GameObject Panel;
    public GameObject Player;
    public bool CanSpawn;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }
    private void OnDestroy()
    {
        // 내가 현재 활성화된 인스턴스였다면, 파괴될 때 참조를 비워줌
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public void SearchPlayer()
    {
        Player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        if (NumberOfKills != lastKillCount)
        {
            lastKillCount = NumberOfKills;
            if (TextKill != null) 
                TextKill.text = lastKillCount.ToString();
        }

        if (PlayerStats.Instance != null && PlayerStats.Instance.level != lastLevel)
        {
            lastLevel = PlayerStats.Instance.level;
            if (Player_Level_Count != null)
                Player_Level_Count.text = lastLevel.ToString();
        }
    }
    public void RequestPause(MonoBehaviour requester, bool pause)
    {
        if(pause)
        {
            if(pauseRequesters.Contains(requester))
            {
                return;
            }
            else
            {
                pauseRequesters.Add(requester);

                IsPause();
            }
        }
        else if(!pause)
        {
            if(pauseRequesters.Contains(requester))
            {
                pauseRequesters.Remove(requester);

                IsPause();
            }
        }
    }
    private void IsPause()
    {
        bool shouldPause = pauseRequesters.Count > 0;

        if (Pause == shouldPause)
            return;

        Pause = shouldPause;
        Time.timeScale = Pause ? 0f : 1f;

        if(Pause)
        {
            if (Panel != null)
                Panel.SetActive(true);
        }
        else
        {
            if (Panel != null)
                Panel.SetActive(false);            
        }
        //PauseChanged?.Invoke(Pause);
    }
    public void ResetPause()
    {
        Pause = false;
        pauseRequesters.Clear();

        Time.timeScale = 1f;

        if (Panel != null)
            Panel.SetActive(false);            
    
    }
    public void SelectCharacter()//int index
    {
        //DontDestroyOnLoad_.Instance.selectedCharacterIndex = index; // Save the selected character index
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.LoadScene("Village");
        else
            SceneManager.LoadScene("Village");
    }
}
