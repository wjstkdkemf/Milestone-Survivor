using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections; // Required for Coroutines

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance;

    public List<Wave> WavesList = new List<Wave>();
    [SerializeField] private TMP_Text WaveText;
    [SerializeField] private Camera playerCamera;
    private Transform playerTransform;
    private float playerSearchTimer = 0f;
    [SerializeField] private float waveTimer;
    [SerializeField] private float StartSpawnTimer;
    private float spawnTimer;

    [SerializeField] private int CurrentWave;
    private int SpawnedEnemys;
    private bool SpawnAll;
    private System.Random random = new System.Random();
    [SerializeField] private bool onlySideSpawn;
    [SerializeField] private List<Transform> spawningPotions;

    private bool LastSpawn = false;
    private bool isClearingStage = false;
    // 벽을 감지하기 위한 레이어 마스크
    [Header("Spawn Validation")]
    [SerializeField] private LayerMask wallLayerMask;
    // 유효한 위치를 찾기 위한 최대 시도 횟수
    [SerializeField] private int maxSpawnAttempts = 10;
    // 스폰 시 확인할 반경 (적 크기에 맞춰 조절)
    [SerializeField] private float spawnCheckRadius = 0.5f;
    [SerializeField] private bool is2DGame = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        //if (GameObject.FindWithTag("Player") == null || !GameManager.Instance.CanSpawn || WavesList.Count == 0)
            //return;
        if (playerTransform == null)
        {
            playerSearchTimer -= Time.fixedDeltaTime;
            
            if (playerSearchTimer <= 0f)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) 
                {
                    playerTransform = playerObj.transform;
                }
                
                playerSearchTimer = 1.0f; 
            }

            if (playerTransform == null) return;
        }

        if (!GameManager.Instance.CanSpawn || WavesList.Count == 0)
            return;

        if (!LastSpawn)
        {
            if (!SpawnAll && spawnTimer <= 0)
            {
                // SpawnEnemy()가 true를 반환(성공)했을 때만 타이머를 리셋
                if (SpawnEnemy())
                {
                    spawnTimer = StartSpawnTimer;
                }
                // (실패하면 spawnTimer는 0 이하로 유지되어 다음 FixedUpdate에 다시 시도)
            }
            else if (SpawnAll && SpawnedEnemys < WavesList[CurrentWave].EnemyNumber)
            {
                SpawnEnemy(); // SpawnAll 모드는 성공 여부와 관계없이 계속 시도
            }
            else
            {
                spawnTimer -= Time.fixedDeltaTime;
            }
        }

        waveTimer -= Time.fixedDeltaTime;

        if (waveTimer <= 0 && !LastSpawn)
        {
            if (CurrentWave >= WavesList.Count - 1)
            {
                LastSpawn = true;
            }
            else
            {
                CurrentWave++;
                GenerateWave();
            }
        }

        if (LastSpawn && GameManager.Instance.activeEnemies == 0 && !isClearingStage)
        {
            StartCoroutine(ClearStageAfterItemCollection());
        }
    }

    public void StartWaves(List<Wave> newWaves)
    {
        if (newWaves == null || newWaves.Count == 0)
        {
            Debug.LogError("New waves list is null or empty.");
            return;
        }

        StopWaves();

        WavesList = new List<Wave>(newWaves); 
        CurrentWave = 0;
        isClearingStage = false;
        GameManager.Instance.CanSpawn = true;
        GenerateWave();
    }

    public void StopWaves()
    {
        GameManager.Instance.CanSpawn = false;
        if(WavesList != null) WavesList.Clear();
        CurrentWave = 0;
        SpawnedEnemys = 0;
        waveTimer = 0;
        spawnTimer = 0;
        LastSpawn = false;
        isClearingStage = false;
        StopAllCoroutines(); // Stop any running coroutines like ClearStageAfterItemCollection
    }

    private IEnumerator ClearStageAfterItemCollection()
    {
        isClearingStage = true;
        GameManager.Instance.AllKill = true;
        GameManager.Instance.Heal = true;

        PlayerXpPickup playerPickup = null;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerPickup = playerObj.GetComponent<PlayerXpPickup>();
        }

        yield return new WaitForSeconds(0.5f);

        /*while (FindObjectsByType<XPCrystal>(FindObjectsSortMode.None).Length > 0 || FindObjectsByType<GoldCoin>(FindObjectsSortMode.None).Length > 0 || FindObjectsByType<ItemObject>(FindObjectsSortMode.None).Length > 0)
        {
            yield return null;
        }*/
        while (true)
        {
            int crystalCount = FindObjectsByType<XPCrystal>(FindObjectsSortMode.None).Length;
            int goldCount = FindObjectsByType<GoldCoin>(FindObjectsSortMode.None).Length;
            int itemCount = FindObjectsByType<ItemObject>(FindObjectsSortMode.None).Length;

            if (crystalCount == 0 && goldCount == 0 && itemCount == 0)
                break;

            if (playerPickup != null)
            {
                playerPickup.CollectEverything();
            }
            else
            {
                // 혹시 루프 도중에 플레이어가 죽어서 null이 됐거나, 처음에 못 찾았다면 다시 찾음
                // (안전장치)
                playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) playerPickup = playerObj.GetComponent<PlayerXpPickup>();
            }

            yield return new WaitForSeconds(0.5f); 
        }
        GameManager.Instance.AllKill = false;

        yield return new WaitForSeconds(1f);

        if(GameOver.Instance != null)
        {
            GameOver.Instance.stageClear(true);
        }
    }

    public void GenerateWave()
    {
        if (WaveText != null)
            WaveText.text = "Wave: " + (CurrentWave + 1).ToString();

        SpawnAll = WavesList[CurrentWave].SpawnAll;
        StartSpawnTimer = WavesList[CurrentWave].SpawnTimer;
        waveTimer = WavesList[CurrentWave].waveDuration;
        SpawnedEnemys = 0;
    }

    bool SpawnEnemy()
    {
        if (WavesList[CurrentWave].Enemys.Count == 0) return false; // 스폰 실패

        Vector3 spawnPosition;
        //TryGet... 함수를 호출하고 실패 시 즉시 false 반환
        if (!TryGetRandomSpawnPosition(out spawnPosition))
        {
            return false; // 위치 찾기 실패 -> 스폰 실패
        }

        GameObject enemyToSpawn = GetRandomEnemy();
        if(enemyToSpawn == null) return false; // 스폰할 적 없음 -> 스폰 실패

        if (!WavesList[CurrentWave].DontUseObjectPooling)
        {
            ObjectPoolingManager.instance.spawnGameObject(enemyToSpawn, spawnPosition, Quaternion.identity);
        }
        else
        {
            Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
        }

        SpawnedEnemys++;
        return true; // 스폰 성공!
    }

    bool TryGetRandomSpawnPosition(out Vector3 spawnPosition)
    {
        if (WavesList[CurrentWave].RandomPostions)
        {
            // out 매개변수는 함수 시작 시 초기화해야 합니다.
            spawnPosition = Vector3.zero;

            for (int i = 0; i < maxSpawnAttempts; i++)
            {
                float randomX, randomY;
                int side = onlySideSpawn ? Random.Range(2, 4) : Random.Range(0, 4);

                switch (side)
                {
                    case 0: randomX = Random.Range(0f, 1f); randomY = 1.2f; break;
                    case 1: randomX = Random.Range(0f, 1f); randomY = -0.2f; break;
                    case 2: randomX = -0.2f; randomY = Random.Range(0f, 1f); break;
                    case 3: randomX = 1.2f; randomY = Random.Range(0f, 1f); break;
                    default: randomX = 0f; randomY = 0f; break;
                }

                if (playerCamera == null) return false;

                spawnPosition = playerCamera.ViewportToWorldPoint(new Vector3(randomX, randomY, 0f));
                spawnPosition.z = 0f;

                bool hitWall = false;
                if (is2DGame)
                {
                    // 2D Physics 체크
                    hitWall = Physics2D.OverlapCircle(spawnPosition, spawnCheckRadius, wallLayerMask) != null;
                }
                else
                {
                    // 3D Physics 체크
                    hitWall = Physics.CheckSphere(spawnPosition, spawnCheckRadius, wallLayerMask);
                }

                if (!hitWall)
                {
                    return true;
                }
            }

            // [수정] 최대 시도 횟수를 초과한 경우
            //Debug.LogWarning($"Failed to find valid spawn position after {maxSpawnAttempts} attempts. Cancelling spawn.");

            // 실패! false 반환
            return false;
        }
        else
        {
            // `spawningPotions`를 사용하는 경우는 항상 성공으로 간주
            //int x = Random.Range(0, spawningPotions.Count);
            if (spawningPotions != null && spawningPotions.Count > 0)
            {
                int x = Random.Range(0, spawningPotions.Count);
                spawnPosition = spawningPotions[x].position;
                return true;
            }
            spawnPosition = Vector3.zero;
            return false;
        }
    }
    
    public GameObject GetRandomEnemy()
    {
        int totalPercentage = 0;
        foreach (var Enemy in WavesList[CurrentWave].Enemys)
        {
            totalPercentage += Enemy.Chance;
        }

        if (totalPercentage == 0) return null;

        int randomValue = random.Next(1, totalPercentage + 1);

        foreach (var Enemy in WavesList[CurrentWave].Enemys)
        {
            if (randomValue <= Enemy.Chance)
            {
                return Enemy.Enemy;
            }
            randomValue -= Enemy.Chance;
        }

        return null;
    }
}

[System.Serializable]
public class Wave
{
    public List<Enemys> Enemys = new List<Enemys>();
    public int waveDuration;
    public float SpawnTimer;
    [Header("Spawn All enemys at ones")]
    public int EnemyNumber;
    public bool SpawnAll;
    public bool RandomPostions = true;
    public bool DontUseObjectPooling;
}

[System.Serializable]
public class Enemys
{
    public GameObject Enemy;
    [Range(0, 100)]
    public int Chance;
}
