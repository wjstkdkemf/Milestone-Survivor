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
    [SerializeField] private float waveTimer;
    [SerializeField] private float StartSpawnTimer;
    private float spawnTimer;

    [SerializeField] private int TheCurantWave;
    private int SpawnedEnemys;
    private bool SpawnAll;
    private System.Random random = new System.Random();
    [SerializeField] private bool onlySideSpawn;
    [SerializeField] private List<Transform> spawningPotions;

    private bool LastSpawn = false;
    private bool isClearingStage = false;

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
        if (GameObject.FindWithTag("Player") == null || !GameManager.Instance.CanSpawn || WavesList.Count == 0)
            return;

        if (WaveText != null)
            WaveText.text = "Wave: " + (TheCurantWave + 1).ToString();

        if (!LastSpawn)
        {
            if (!SpawnAll && spawnTimer <= 0)
            {
                SpawnEnemy();
                spawnTimer = StartSpawnTimer;
            }
            else if (SpawnAll && SpawnedEnemys < WavesList[TheCurantWave].EnemyNumber)
            {
                SpawnEnemy();
            }
            else
            {
                spawnTimer -= Time.fixedDeltaTime;
            }
        }

        waveTimer -= Time.fixedDeltaTime;

        if (waveTimer <= 0 && !LastSpawn)
        {
            if (TheCurantWave >= WavesList.Count - 1)
            {
                LastSpawn = true;
            }
            else
            {
                TheCurantWave++;
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

        StopWaves(); // Reset everything before starting

        WavesList = new List<Wave>(newWaves); // Create a new list to avoid modifying the original
        TheCurantWave = 0;
        isClearingStage = false;
        GameManager.Instance.CanSpawn = true;
        GenerateWave();
    }

    public void StopWaves()
    {
        GameManager.Instance.CanSpawn = false;
        if(WavesList != null) WavesList.Clear();
        TheCurantWave = 0;
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

        yield return new WaitForSeconds(0.5f);

        while (FindObjectsByType<XPCrystal>(FindObjectsSortMode.None).Length > 0 || FindObjectsByType<GoldCoin>(FindObjectsSortMode.None).Length > 0 || FindObjectsByType<ItemObject>(FindObjectsSortMode.None).Length > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        if(GameOver.Instance != null)
        {
            GameOver.Instance.stageClear(true);
        }
    }

    public void GenerateWave()
    {
        SpawnAll = WavesList[TheCurantWave].SpawnAll;
        StartSpawnTimer = WavesList[TheCurantWave].SpawnTimer;
        waveTimer = WavesList[TheCurantWave].waveDuration;
        SpawnedEnemys = 0;
    }

    void SpawnEnemy()
    {
        if (WavesList[TheCurantWave].Enemys.Count == 0) return;

        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject enemyToSpawn = GetRandomEnemy();

        if(enemyToSpawn == null) return;

        if (!WavesList[TheCurantWave].DontUseObjectPooling)
        {
            ObjectPoolingManager.instance.spawnGameObject(enemyToSpawn, spawnPosition, Quaternion.identity);
        }
        else
        {
            Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
        }
        SpawnedEnemys++;
    }

    Vector3 GetRandomSpawnPosition()
    {
        if (WavesList[TheCurantWave].RandomPostions)
        {
            float randomX, randomY;
            int side = onlySideSpawn ? Random.Range(2, 4) : Random.Range(0, 4);

            switch (side)
            {
                case 0: randomX = Random.Range(0f, 1f); randomY = 1.2f; break; // Top
                case 1: randomX = Random.Range(0f, 1f); randomY = -0.2f; break; // Bottom
                case 2: randomX = -0.2f; randomY = Random.Range(0f, 1f); break; // Left
                case 3: randomX = 1.2f; randomY = Random.Range(0f, 1f); break; // Right
                default: randomX = 0f; randomY = 0f; break;
            }

            Vector3 spawnPosition = playerCamera.ViewportToWorldPoint(new Vector3(randomX, randomY, 0f));
            spawnPosition.z = 0f;
            return spawnPosition;
        }
        else
        {
            int x = Random.Range(0, spawningPotions.Count);
            return spawningPotions[x].position;
        }
    }

    public GameObject GetRandomEnemy()
    {
        int totalPercentage = 0;
        foreach (var Enemy in WavesList[TheCurantWave].Enemys)
        {
            totalPercentage += Enemy.Chance;
        }

        if (totalPercentage == 0) return null;

        int randomValue = random.Next(1, totalPercentage + 1);

        foreach (var Enemy in WavesList[TheCurantWave].Enemys)
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
