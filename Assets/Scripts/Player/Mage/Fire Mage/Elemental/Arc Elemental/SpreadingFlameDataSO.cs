using UnityEngine;

[CreateAssetMenu(fileName = "New Spreading Flame Data", menuName = "Weapon Data/Mage/Fire Mage/Elemental/Arc Elemental/Spreading Flame Weapon")]
public class SpreadingFlameDataSO : WeaponDataSO
{
    [Header("Spreading Flame Settings")]
    public float searchRadius = 8f;
    public int flameCount = 1;
    public float flameDuration = 5f;
    public float tickInterval = 1f;
    public float spreadRadius = 5f;
    public float spreadDelay = 0f;
    public long damagePerTick = 5;
    public GameObject flameEffectPrefab;

    [Header("Level Up")]
    public int flameCountIncrease = 1;
    public long damageIncrease = 2;
    public float spreadRadiusIncrease = 0.5f;
}
