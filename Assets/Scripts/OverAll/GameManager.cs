
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool StopMoveing;
    public bool Pause;
    public TMP_Text TextKill;
    public TMP_Text Player_Level_Count;
    public int NumberOfKills;
    public int activeEnemies = 0;
    private int lastKillCount = -1;
    private int lastLevel = -1;
    public bool AllKill = false;
    public bool Heal = false;
    public GameObject Panel;
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

    void Update()
    {
        if (NumberOfKills != lastKillCount)
        {
            lastKillCount = NumberOfKills;
            if (TextKill != null) 
                TextKill.text = lastKillCount.ToString();
        }

        if (PlayerStats.Instance.level != lastLevel)
        {
            lastLevel = PlayerStats.Instance.level;
            if (Player_Level_Count != null)
                Player_Level_Count.text = lastLevel.ToString();
        }

        if (Pause)
        {
            if (Panel != null)
                Panel.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            if (Panel != null)
                Panel.SetActive(false);
            Time.timeScale = 1;
        }
    }
    public void SelectCharacter()//int index
    {
        //DontDestroyOnLoad_.Instance.selectedCharacterIndex = index; // Save the selected character index
        SceneManager.LoadScene("Village");
    }
}