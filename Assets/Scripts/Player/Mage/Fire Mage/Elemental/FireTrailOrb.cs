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
    public void SetTrailInfo(GameObject prefab, float distThreshold, float duration, float damage , string WeaponID)
    {
        trailPrefab = prefab;
        spawnDistanceThreshold = distThreshold;
        trailDuration = duration;
        trailDamage = damage;

        this.WeaponID = WeaponID;

        lastSpawnPosition = transform.position; 
        isFirstSpawn = true; // 스폰 시 초기화
    }
    protected override void Update()
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.CanProcessCombat)
            return;

        base.Update();
        bool shouldSpawn = isFirstSpawn ||
            Vector3.Distance(transform.position, lastSpawnPosition)
                >= spawnDistanceThreshold;

        if (shouldSpawn && SpawnTrail())
        {
            lastSpawnPosition = transform.position;
            isFirstSpawn = false;
        }
    }
    private bool SpawnTrail()
    {
        var pool = ObjectPoolingManager.Instance;
        if (trailPrefab == null || pool == null)
            return false;

        GameObject trail = pool.spawnGameObject(
            trailPrefab, transform.position, Quaternion.identity);

        if (trail == null)
            return false;

        if (!trail.TryGetComponent<AuraZone>(out var trailSkill))
        {
            pool.ReturnObjectToPool(trail);
            return false;
        }

        trailSkill.SetupAura(
            0.5f, (long)trailDamage, 1.5f,
            false, 0f, WeaponID, trailDuration);

        return true;
    }
}
