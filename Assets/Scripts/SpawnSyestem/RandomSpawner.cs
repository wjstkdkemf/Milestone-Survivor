using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;
    public Transform spawnCenter;
    public float spawnRadius = 3f;
    private float Timer;
    public float StartTimer = 2;
    public int SpawnNumber;
    private void Update()
    {
        Timer -= Time.deltaTime;
        if (Timer <= 0 && SpawnNumber > 0)
        {
            for (int i = 0; i < SpawnNumber; i++)
            {
                SpawnObjects();
            }

            Timer = StartTimer;
        }
    }
    void SpawnObjects()
    {

        Vector3 randomPosition = RandomPositionInCircle();      
        ObjectPoolingManager.instance.spawnGameObject(objectToSpawn, randomPosition, Quaternion.identity); // spawn first enemy in our list

    }

    Vector3 RandomPositionInCircle()
    {
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(0f, spawnRadius);
        float randomY = Random.Range(-spawnRadius, spawnRadius); // Random Y within circle
        Vector3 randomPosition = spawnCenter.position + Quaternion.Euler(0, angle, 0) * (new Vector3(distance, randomY, 0));
        return randomPosition;
    }
}
