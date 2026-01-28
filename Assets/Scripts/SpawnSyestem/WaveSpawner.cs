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
    private bool SpawnCircle;
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
    [SerializeField] private float circleRadius = 10f;//일괄 스폰용
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
        if(GameManager.Instance.Pause)
            return;

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
            else if(SpawnAll && SpawnCircle && SpawnedEnemys < WavesList[CurrentWave].EnemyNumber)
            {
                SpawnCirclePattern(WavesList[CurrentWave].EnemyNumber);
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
                // 마지막 웨이브 시간이 끝나면 즉시 스폰을 금지시킵니다.
                //GameManager.Instance.CanSpawn = false; 
                Debug.Log("모든 웨이브 스폰 종료. 몬스터 전멸 대기 중...");
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
            /*            // 몬스터가 다 잡혔는지 확인
            if (GameManager.Instance.activeEnemies <= 0)
            {
                // 혹시 음수가 될 수도 있으니 <= 0 으로 체크
                GameManager.Instance.activeEnemies = 0; 
                StartCoroutine(ClearStageAfterItemCollection());
            }*/
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
        SpawnCircle = WavesList[CurrentWave].SpawnCircle;
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
    public void SpawnCirclePattern(int count)
    {
        if (playerTransform == null) return;


        GameObject enemyPrefab = GetRandomEnemy();
        if (enemyPrefab == null) return;

        Vector3 center = playerTransform.position;
        float angleStep = 360f / count; // 몬스터 간의 각도 간격

        for (int i = 0; i < count; i++)
        {
            // 1. 원형 좌표 계산 (삼각함수)
            float angle = i * angleStep * Mathf.Deg2Rad; // 라디안 변환
            float x = Mathf.Cos(angle) * circleRadius;
            float y = Mathf.Sin(angle) * circleRadius;
            
            Vector3 spawnPos = center + new Vector3(x, y, 0);

            // 2. 유효한 위치인지 확인 (NavMesh 위인지, 벽 안인지)
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPos = hit.position; // NavMesh 위로 보정
            }
            else
            {
                continue; // 길을 못 찾으면 이번 몬스터는 스킵 (벽 속에 생성 방지)
            }
            Debug.Log(spawnPos);
            // 3. 몬스터 소환 (풀링 사용)
            // 전용 몬스터 프리팹(enemyPrefab)을 그대로 소환만 하면 됩니다.
            if (!WavesList[CurrentWave].DontUseObjectPooling)
            {
                ObjectPoolingManager.instance.spawnGameObject(enemyPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }


            SpawnedEnemys++;
        }
        
        Debug.Log($"포위망 생성 완료: {count}마리");
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
    public bool SpawnCircle;
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
