using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RunStatisticsManager : MonoBehaviour
{
    public static RunStatisticsManager Instance { get; private set; }

    [SerializeField]private Dictionary<string, WeaponDamageRecord> weaponDamageRecords = new();

    [Header("Debug View")]
    [SerializeField] private List<WeaponDamageRecordDebugView> weaponDamageDebugList = new();


    [SerializeField]private int EncounterCount;
    [SerializeField]private int ClearedEncounterCount;
    [SerializeField]private int KillCount;
    [SerializeField]private long EarnedGold;

    [Serializable]
    public class WeaponDamageRecordDebugView
    {
        public string weaponId;
        public long totalDamage;
        public long highestSingleHit;
        public int hitCount;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void ResetRun()
    {
        weaponDamageRecords.Clear();

        EncounterCount = 0;
        ClearedEncounterCount = 0;
        KillCount = 0;
        EarnedGold = 0;
    }

    public void AddEncounter()
    {
        EncounterCount++;
    }

    public void AddClearedEncounter()
    {
        ClearedEncounterCount++;
    }

    public void AddKill()
    {
        KillCount++;
    }

    public void AddGold(long amount)
    {
        if (amount <= 0)
            return;

        EarnedGold += amount;
    }

    public void RecordWeaponDamage(string weaponId, long damage)
    {
        if (string.IsNullOrEmpty(weaponId))
            return;

        if (damage <= 0)
            return;

        if (!weaponDamageRecords.TryGetValue(weaponId, out WeaponDamageRecord record))
        {
            record = new WeaponDamageRecord(weaponId);
            weaponDamageRecords.Add(weaponId, record);
        }

        record.AddDamage(damage);

        RefreshDebugList();
    }

    public RunResultData CreateResultData()
    {
        WeaponDamageRecord bestTotalDamageWeapon = null;
        WeaponDamageRecord bestSingleHitWeapon = null;

        foreach (var record in weaponDamageRecords.Values)
        {
            if (bestTotalDamageWeapon == null ||
                record.TotalDamage > bestTotalDamageWeapon.TotalDamage)
            {
                bestTotalDamageWeapon = record;
            }

            if (bestSingleHitWeapon == null ||
                record.HighestSingleHit > bestSingleHitWeapon.HighestSingleHit)
            {
                bestSingleHitWeapon = record;
            }
        }

        return new RunResultData
        {
            EncounterCount = EncounterCount,
            ClearedEncounterCount = ClearedEncounterCount,
            KillCount = KillCount,
            EarnedGold = EarnedGold,

            //BestDamageWeaponName = bestTotalDamageWeapon?.WeaponName ?? "-", // 추후 수정
            BestDamageWeaponTotalDamage = bestTotalDamageWeapon?.TotalDamage ?? 0,

            //HighestSingleHitWeaponName = bestSingleHitWeapon?.WeaponName ?? "-",
            HighestSingleHitDamage = bestSingleHitWeapon?.HighestSingleHit ?? 0
        };
    }
    private void RefreshDebugList()
    {
        weaponDamageDebugList.Clear();

        foreach (var pair in weaponDamageRecords)
        {
            WeaponDamageRecord record = pair.Value;

            weaponDamageDebugList.Add(new WeaponDamageRecordDebugView
            {
                weaponId = record.WeaponId,
                totalDamage = record.TotalDamage,
                highestSingleHit = record.HighestSingleHit,
                hitCount = record.HitCount
            });
        }
    }
}

public class RunResultData
{
    public int EncounterCount;
    public int ClearedEncounterCount;
    public int KillCount;
    public long EarnedGold;

    public string BestDamageWeaponName;
    public long BestDamageWeaponTotalDamage;

    public string HighestSingleHitWeaponName;
    public long HighestSingleHitDamage;

    public string MostUsedSkillName;
    public int MostUsedSkillCount;
}