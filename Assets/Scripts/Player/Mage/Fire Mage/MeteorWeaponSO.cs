using UnityEngine;

[CreateAssetMenu(fileName = "New Meteor Data", menuName = "Weapon Data/Mage/Fire Mage/Meteor Weapon")]
public class MeteorWeaponSO : WeaponDataSO
{
    [Header("Meteor Specific Stats")]
    public int MeteorNumber = 1;          // 발사체 개수
    public float range = 15f;              // 사거리 (Distance)
    public float densityCheckRadius = 4.0f;
    public float targetUpdateRate = 0.5f; // 타겟 검색 주기
    public float warningDuration = 1f;
    public float volleyDelay = 0.3f; 
    
    [Header("Damage Settings")]
    public float playerDamageScaling = 0.5f; // 플레이어 스탯 반영 비율

    [Header("Setup")]
    public GameObject MeteorPrefab;       // 메테오 프리팹
    public GameObject magicCirclePrefab;
    public LayerMask enemyLayerMask;      // 적 레이어
}