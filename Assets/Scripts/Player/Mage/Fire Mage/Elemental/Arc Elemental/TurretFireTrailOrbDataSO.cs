using UnityEngine;

[CreateAssetMenu(fileName = "New Turret Fire Trail Orb Data", menuName = "Weapon Data/Mage/Fire Mage/Elemental/Arc Elemental/Turret Fire Trail Orb Weapon")]
// [핵심 설명] FireTrailOrbDataSO를 상속받으므로, 장판(Trail) 설정 변수들도 인스펙터에 그대로 나타납니다.
public class TurretFireTrailOrbDataSO : FireTrailOrbDataSO
{
    [Header("Turret Settings (포탑 설정)")]
    [Tooltip("발사할 미사일 프리팹 (TurretBullet, DoDamage 포함)")]
    public GameObject projectilePrefab;
    
    [Tooltip("미사일 발사 쿨타임")]
    public float fireRate = 1.5f;
    
    [Tooltip("적 탐지 사거리 (오브 기준)")]
    public float fireRange = 8f;
    
    [Tooltip("미사일 데미지 배율 (오브 기본 데미지의 몇 %인가)")]
    public float projectileDamageScaling = 1.0f;

    [Tooltip("적 탐지용 레이어")]
    public LayerMask enemyLayerMask;
}