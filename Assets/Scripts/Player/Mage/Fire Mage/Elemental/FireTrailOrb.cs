using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrailOrb : Orb
{
    private GameObject trailPrefab;

    private float spawnDistanceThreshold;
    private float trailDuration;
    private float trailDamage;

    private Vector3 lastSpawnPosition;
    private bool isFirstSpawn = true;

    // 무기(Weapon)에서 호출하여 장판 관련 정보 주입
    public void SetTrailInfo(GameObject prefab, float distThreshold, float duration, float damage)
    {
        trailPrefab = prefab;
        spawnDistanceThreshold = distThreshold;
        trailDuration = duration;
        trailDamage = damage;

        lastSpawnPosition = transform.position; 
        isFirstSpawn = true; // 스폰 시 초기화
    }
    protected override void Update()
    {
        base.Update();

        if (isFirstSpawn)
        {
            SpawnTrail();
            lastSpawnPosition = transform.position;
            isFirstSpawn = false;
        }
        if (Vector3.Distance(transform.position, lastSpawnPosition) >= spawnDistanceThreshold)
        {
            SpawnTrail();
            lastSpawnPosition = transform.position; 
        }
    }
    private void SpawnTrail()
    {
        if (trailPrefab == null) return;

        GameObject trail = ObjectPoolingManager.Instance.spawnGameObject(trailPrefab, transform.position, Quaternion.identity);

        if (trail != null && trail.TryGetComponent<AuraZone>(out var trailSkill))
        {
            trailSkill.SetupAura(0.5f, trailDamage, 1.5f, false, 0f, trailDuration); 
        }
    }
}
