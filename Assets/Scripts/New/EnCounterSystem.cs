using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public MapMaker currentMap;
    private Vector2 lastPos;
    private float walkedDistance = 0.0f;
    private Vector2 enCounterPos;

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
        }
    }

    void Start()
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
    }

    void Update()
    {
        if (currentMap != null && !isEncounterActive && CurEncounter < maxEncounter)
        {
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
        lastPos = PlayerTransform.position;
        walkedDistance = 0.0f;
    }

    public void ExitMap()
    {
        currentMap = null;
    }

    void StartEncount()
    {
        if (currentMap == null || tilemapManager == null || waveSpawner == null)
        {
            Debug.LogError("Cannot start encounter: a required component is missing.");
            return;
        }
        isEncounterActive = true;
        enCounterPos = PlayerTransform.position; // Save player's current position

        // 1. Generate the battle map at a distant location
        tilemapManager.GenerateMap(currentMap.SceneName); // SceneName is used as the map theme

        // 2. Start the monster waves defined in the MapMaker
        waveSpawner.StartWaves(currentMap.waves);

        // 3. Activate combat abilities
        if (UpgradeManager.Instance != null) UpgradeManager.Instance.SetCombatState(true);

        CurEncounter++;
    }

    public void ClearEncount()
    {
        // Optional: Save stats if needed
        // PlayerStats.Instance.SaveStats();

        if (CurEncounter >= maxEncounter)
        {
            if(GameOver.Instance != null) GameOver.Instance.GameEnded(true);
        }
        else
        {
            // 1. Clear the battle map
            if(tilemapManager != null) tilemapManager.ClearMap();

            // 2. Stop the monster spawner
            if(waveSpawner != null) waveSpawner.StopWaves();

            // 3. Deactivate combat abilities
            if (UpgradeManager.Instance != null) UpgradeManager.Instance.SetCombatState(false);

            // 4. Teleport player back to where the encounter started
            if(PlayerTransform != null) PlayerTransform.position = enCounterPos;
            isEncounterActive = false;
        }
    }
}
