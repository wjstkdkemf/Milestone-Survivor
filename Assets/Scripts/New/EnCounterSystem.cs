using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnCounterSystem : MonoBehaviour
{
    public static EnCounterSystem Instance { get; private set; }

    [Header("플레이어 설정")]
    public Transform PlayerTransform;

    [Header("인카운트 설정")]
    [Range(0, 100)] public float encountpercent = 10.0f;
    public float setpDistance = 1.0f;
    public int maxEncounter = 2;
    private int CurEncounter = 0;


    //public List<Enemys> Monster;
    public MapMaker currentMap;
    private Vector2 lastPos;
    private float walkedDistance = 0.0f;
    private static Vector2 enCounterPos;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += TeleportPlayer;
    }

    void TeleportPlayer(Scene arg0, LoadSceneMode arg1)
    {
        if (PlayerTransform == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                Debug.Log("플레이어 위치를 찾음");
                PlayerTransform = playerObject.transform;
            }
            else
            {
                Debug.Log("플레이어 위치를 못찾음");
            }
        }
        else
        {
            lastPos = PlayerTransform.position;
        }

        if (arg0.name == "Map 1") // SceneManager.GetActiveScene()
        {
            Debug.Log(enCounterPos);
            PlayerTransform.position = enCounterPos;
        }
    }

    void Start()
    {
        if (PlayerTransform == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                PlayerTransform = playerObject.transform;
            }
            else
            {
                Debug.Log("플레이어 위치를 못찾음");
            }
        }
        else
        {
            lastPos = PlayerTransform.position;
        }
    }

    void Update()
    {
        if (currentMap != null && CurEncounter < maxEncounter)
        {
            float currentMoveDistance = Vector2.Distance(PlayerTransform.position, lastPos);
            walkedDistance += currentMoveDistance;
            lastPos = PlayerTransform.position;


            if (walkedDistance >= setpDistance)
            {
                walkedDistance -= setpDistance;

                if (Random.Range(0.0f, 100.0f) < encountpercent)
                {
                    enCounterPos = PlayerTransform.position;
                    StartEncount();
                }
            }
        }
    }

    public void EnterMap(MapMaker map)
    {
        currentMap = map;
        lastPos = PlayerTransform.position;
        walkedDistance = 0.0f;
    }

    public void ExitMap()
    {
        currentMap = null;
    }

    void StartEncount()
    {
        CurEncounter++;

        SceneManager.LoadScene(currentMap.SceneName);
    }

    public void ClearEncount()
    {
        PlayerStats.Instance.SaveStats();

        if (CurEncounter == maxEncounter)
        {
            GameOver.Instance.GameEnded(true);
        }
        else
        {
             SceneManager.LoadScene("Map 1");
        }
    }
}
