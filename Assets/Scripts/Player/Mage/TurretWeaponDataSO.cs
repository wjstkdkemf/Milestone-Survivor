using UnityEngine;

[CreateAssetMenu(fileName = "New Turret Data", menuName = "Weapon Data/Turret Weapon")]
public class TurretWeaponDataSO : WeaponDataSO
{
    [Header("Turret Specific Stats")]
    public int bulletNumber = 1;          // 발사체 개수
    public float fireRate = 1f;           // 공격 쿨타임 (AttackBase의 cooldown)
    public float range = 5f;              // 사거리 (Distance)
    public float targetUpdateRate = 0.5f; // 타겟 검색 주기
    
    [Header("Damage Settings")]
    public float playerDamageScaling = 0.1f; // 플레이어 스탯 반영 비율

    [Header("Setup")]
    public GameObject bulletPrefab;       // 총알 프리팹
    public LayerMask enemyLayerMask;      // 적 레이어
}