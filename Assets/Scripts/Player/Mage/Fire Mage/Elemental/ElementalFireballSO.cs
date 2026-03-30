using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Elemental Fireball Data", menuName = "Weapon Data/Mage/Fire Mage/Elemental/Elemental Fireball")]
public class ElementalFireballSO : WeaponDataSO
{
    [Header("Fireball Stats")]
    public int bulletNumber = 1;
    public float range = 10f;
    public float projectileSpeed = 12f;

    [Header("Trail Stats (장판 설정)")]
    [Tooltip("장판 데미지 계수 (기본 데미지의 몇 %인가)")]
    public float trailDamageScaling = 0.5f; 
    [Tooltip("장판 지속 시간")]
    public float trailDuration = 3f;
    [Tooltip("장판 생성 간격 (이동 거리 기준, 낮을수록 촘촘함)")]
    public float trailSpawnDistance = 0.8f;
    [Tooltip("최후 폭발 크기")]
    public float FireBoomSize = 3.0f;
    [Header("Setup")]
    public GameObject fireballPrefab; // 날아가는 투사체 프리팹
    public GameObject trailPrefab;    // 바닥에 깔릴 장판 프리팹 (DoDamage 부착 필수)
    public GameObject lastFireBoomPrefab;
    public LayerMask enemyLayerMask;
}
