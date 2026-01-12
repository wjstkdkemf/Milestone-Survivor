using UnityEngine;
using UnityEngine.SceneManagement; // 1. SceneManagement 네임스페이스 추가
using System.Collections;
using System.Collections.Generic;

public class EnCounterSystem : MonoBehaviour
{
    public static EnCounterSystem Instance { get; private set; }

    [Header("플레이어 설정")]
    public Transform PlayerTransform;

    [Header("인카운트 설정")]
    [Range(0, 100)] public float encountpercent = 10.0f;
    public float setpDistance = 1.0f;
    public int normalMaxEncounter = 3;
    public int maxEncounter = 0;
    private int CurEncounter = 0;

    public MapMaker currentMap;
    private Vector3 lastPos;
    private float walkedDistance = 0.0f;
    private Vector3 enCounterPos;

    // System References
    private InfiniteTilemapManager tilemapManager;
    private WaveSpawner waveSpawner;
    private bool isEncounterActive = false;

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
            // 2. 씬 로드 이벤트에 구독(Subscribe)
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    // 3. OnDestroy에서 이벤트 구독을 해제(Unsubscribe)하여 메모리 누수 방지
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 4. 씬이 로드될 때마다 호출될 메소드
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        maxEncounter = 0;
        maxEncounter += normalMaxEncounter;
        Debug.Log(scene.name + " 씬이 로드되었습니다. EnCounterSystem을 초기화합니다.");
        if(SceneManager.GetActiveScene().name == "GameplayScene")
            InitializeSceneComponents();
    }

    // 5. 기존 Start()의 내용을 별도 메소드로 분리
    void InitializeSceneComponents()
    {
        // Find the player
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            PlayerTransform = playerObject.transform;
            lastPos = PlayerTransform.position;
        }
        else
        {
            Debug.LogError("Player not found!");
        }

        // Get references to the managers
        tilemapManager = FindObjectOfType<InfiniteTilemapManager>();
        if (tilemapManager == null)
        {
            Debug.LogError("InfiniteTilemapManager not found!");
        }

        waveSpawner = WaveSpawner.Instance;
        if (waveSpawner == null)
        {
            Debug.LogError("WaveSpawner instance not found!");
        }

        // 씬이 바뀔 때마다 인카운트 횟수를 초기화하고 싶다면 아래 주석을 해제하세요.
        CurEncounter = 0;
        isEncounterActive = false;
    }

    // 6. 기존 Start() 메소드는 이제 필요 없으므로 삭제하거나 비워둡니다.
    // void Start() { }

    void Update()
    {
        if (currentMap != null && !isEncounterActive && CurEncounter < maxEncounter)
        {
            if (PlayerTransform == null) return; // 플레이어를 못찾았으면 Update 로직 중지

            float currentMoveDistance = Vector2.Distance(PlayerTransform.position, lastPos);
            walkedDistance += currentMoveDistance;
            lastPos = PlayerTransform.position;

            if (walkedDistance >= setpDistance)
            {
                walkedDistance -= setpDistance;

                if (Random.Range(0.0f, 100.0f) < encountpercent)
                {
                    StartEncount();
                }
            }
        }
    }

    public void EnterMap(MapMaker map)
    {
        currentMap = map;
        if (PlayerTransform != null)
        {
            lastPos = PlayerTransform.position;
        }
        walkedDistance = 0.0f;
    }

    public void ExitMap()
    {
        currentMap = null;
    }
    public void BossEncount()
    {
        lastPos = PlayerTransform.position;
        StartEncount();
    }

    public void StartEncount()
    {
        StartCoroutine(StartEncountCoroutine());
    }

    IEnumerator StartEncountCoroutine()
    {
        if (currentMap == null || tilemapManager == null || waveSpawner == null)
        {
            Debug.LogError("Cannot start encounter: a required component is missing.");
            yield break;
        }
        string SceneName = currentMap.SceneName;
        List<Wave> SceneWave = new List<Wave>(currentMap.waves);

        isEncounterActive = true;
        if(MenuButtonController.Instance.Inventory && MenuButtonController.Instance.ingame)
        {
            MenuButtonController.Instance.back();
        }
        enCounterPos = PlayerTransform.position; // Save player's current position

        // 1. Generate the battle map and move the player
        tilemapManager.GenerateMap(SceneName);

        // 2. Wait for the next frame to ensure the camera has updated its position
        yield return null;

        // 3. Start the monster waves
        waveSpawner.StartWaves(SceneWave);

        // 4. Activate combat abilities
        if (UpgradeManager.Instance != null) UpgradeManager.Instance.SetCombatState(true);

        CurEncounter++;
    }

    public void ClearEncount()
    {
        // Optional: Save stats if needed
        // PlayerStats.Instance.SaveStats();

        if (CurEncounter >= maxEncounter)
        {
            if (GameOver.Instance != null) GameOver.Instance.GameEnded(true);
        }
        else
        {
            // 1. Clear the battle map
            if (tilemapManager != null) tilemapManager.ClearMap();

            // 2. Stop the monster spawner
            if (waveSpawner != null) waveSpawner.StopWaves();

            // 3. Deactivate combat abilities
            if (UpgradeManager.Instance != null) UpgradeManager.Instance.SetCombatState(false);

            // 4. Teleport player back to where the encounter started
            if (PlayerTransform != null) PlayerTransform.position = enCounterPos;
            isEncounterActive = false;
        }
        if(currentMap.BossEncounter)
            currentMap = null;
    }
    public void PlusMaxEncount(int PlusEn)
    {
        maxEncounter += PlusEn;
    }
}