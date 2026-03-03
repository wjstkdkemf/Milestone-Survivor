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
            //DontDestroyOnLoad(gameObject);
        }
    }
    public void InitializeSceneComponents()
    {
        Debug.Log("EnCounterSystem을 초기화합니다.");
        maxEncounter = 0;
        maxEncounter += normalMaxEncounter;
       
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

        CurEncounter = 0;
        isEncounterActive = false;
    }

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

        tilemapManager.GenerateMap(SceneName);

        // 1. Generate the battle map and move the player
        yield return StartCoroutine(waveSpawner.PreloadWaveAssets(SceneWave));

        // 카메라 위치 업데이트 등, 프레임 안정화를 위해 한 프레임 대기
        yield return null;

        waveSpawner.StartWaves();
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
            if (waveSpawner != null) waveSpawner.ReleaseWaveAssets();

            // 3. Deactivate combat abilities
            if (UpgradeManager.Instance != null) UpgradeManager.Instance.SetCombatState(false);

            // 4. Teleport player back to where the encounter started
            if (PlayerTransform != null) PlayerTransform.position = enCounterPos;
            isEncounterActive = false;
        }
        if(currentMap != null && currentMap.BossEncounter)
            currentMap = null;
    }
    public void PlusMaxEncount(int PlusEn)
    {
        maxEncounter += PlusEn;
    }
}