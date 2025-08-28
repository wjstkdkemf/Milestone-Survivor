using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections; // Required for Coroutines

public class WaveSpawner : MonoBehaviour
{
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

    private int totalMonster = 0;

    private bool LastSpawn = false;
    private bool isClearingStage = false; // Flag to ensure the clear coroutine runs only once

    void Start()
    {
        totalMonster += WavesList[0].EnemyNumber;
    }

    void FixedUpdate()
    {
        if (GameObject.FindWithTag("Player") == null || !GameManager.Instance.CanSpawn)
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
            else if (SpawnAll && SpawnedEnemys <= WavesList[TheCurantWave].EnemyNumber)
            {
                SpawnEnemy();
            }
            else
            {
                spawnTimer -= Time.fixedDeltaTime;
                waveTimer -= Time.fixedDeltaTime;
            }
        }

        if (waveTimer <= 0)
        {
            if (TheCurantWave >= WavesList.Count - 1)
            {
                LastSpawn = true;

                if (GameManager.Instance.activeEnemies == 0 && !isClearingStage)
                {
                    StartCoroutine(ClearStageAfterItemCollection());
                }
            }
            else
            {
                SpawnedEnemys = 0;
                TheCurantWave++;
                GenerateWave();
            }
        }
    }

    private IEnumerator ClearStageAfterItemCollection()
    {
        isClearingStage = true;

        // Trigger the collection of all items in PlayerXpPickup
        GameManager.Instance.AllKill = true;

        // Wait a brief moment to ensure the collection process has started
        yield return new WaitForSeconds(0.5f);

        // Wait until all XPCrystal and GoldCoin objects in the scene are collected
        while (FindObjectsOfType<XPCrystal>().Length > 0 || FindObjectsOfType<GoldCoin>().Length > 0)
        {
            yield return null; // Wait for the next frame
        }

        // Wait for 1 second after all items are collected
        yield return new WaitForSeconds(1f);

        // Now, proceed to clear the stage
        if(GameOver.Instance != null)
        {
            GameOver.Instance.stageClear(true);
        }
    }

    public void GenerateWave()
    {
        SpawnAll = WavesList[TheCurantWave].SpawnAll;
        if (WavesList[TheCurantWave].SpawnAll == false)
        {
            StartSpawnTimer = WavesList[TheCurantWave].SpawnTimer;
        }
        else
        {
            StartSpawnTimer = 0;
        }
        waveTimer = WavesList[TheCurantWave].waveDuration;
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        if (!WavesList[TheCurantWave].DontUseObjectPooling)
        {
            ObjectPoolingManager.instance.spawnGameObject(GetRandomEnemy(), spawnPosition, Quaternion.identity);
            SpawnedEnemys++;
        }
        else
        {
            Instantiate(GetRandomEnemy());
            SpawnedEnemys++;
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        if (WavesList[TheCurantWave].RandomPostions)
        {
            float randomX, randomY;
            int side = 0; // 0: top, 1: bottom, 2: left, 3: right
            if (onlySideSpawn)
                side = Random.Range(2, 4);
            else
                side = Random.Range(0, 4);

            switch (side)
            {
                case 0: // Top
                    randomX = Random.Range(0f, 1f);
                    randomY = 1.2f; // Adjust this value to spawn above the camera view
                    break;

                case 1: // Bottom
                    randomX = Random.Range(0f, 1f);
                    randomY = -0.2f; // Adjust this value to spawn below the camera view
                    break;

                case 2: // Left
                    randomX = -0.2f; // Adjust this value to spawn left of the camera view
                    randomY = Random.Range(0f, 1f);
                    break;

                case 3: // Right
                    randomX = 1.2f; // Adjust this value to spawn right of the camera view
                    randomY = Random.Range(0f, 1f);
                    break;

                default:
                    randomX = randomY = 0f;
                    break;
            }

            Vector3 spawnPosition = playerCamera.ViewportToWorldPoint(new Vector3(randomX, randomY, 0f));
            spawnPosition.z = 0f; // Ensure the Z-coordinate is appropriate for your 2D setup

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

        int randomValue = random.Next(1, totalPercentage + 1);

        foreach (var Enemy in WavesList[TheCurantWave].Enemys)
        {
            if (randomValue <= Enemy.Chance)
            {
                totalMonster++;
                return Enemy.Enemy;
            }
            randomValue -= Enemy.Chance;
        }

        return WavesList[1].Enemys[0].Enemy;
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