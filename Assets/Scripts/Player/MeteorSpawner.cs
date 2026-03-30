using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class MeteorSpawner : MonoBehaviour
    {
        public GameObject meteorPrefab; // Reference to the meteor prefab
        public float spawnRate = 1f; // Time between spawns
        public float spawnHeight = 10f; // Height at which meteors will spawn

        private float nextSpawnTime;
        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
        }

        void Update()
        {
            if (Time.time >= nextSpawnTime)
            {
                SpawnMeteor();
                nextSpawnTime = Time.time + 1f / spawnRate;
            }
        }

        void SpawnMeteor()
        {
            float minX = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
            float maxX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
            float spawnX = Random.Range(minX, maxX);
            Vector3 spawnPosition = new Vector3(spawnX, spawnHeight, 0);
            Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
        }
    }