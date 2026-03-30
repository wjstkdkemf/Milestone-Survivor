using UnityEngine;


[CreateAssetMenu(fileName = "New Fire Trail Orb Data", menuName = "Weapon Data/Mage/Fire Mage/Elemental/Fire Trail Orb Weapon")]
public class FireTrailOrbDataSO : OrbWeaponDataSO
{
    [Header("Trail Settings (장판 설정)")]
    [Tooltip("생성될 장판 프리팹 (DoDamage 포함, SelfDestroy 켜져있어야 함)")]
    public GameObject trailPrefab;
    
    [Tooltip("장판 생성 주기 (초 단위, 낮을수록 촘촘하게 생성)")]
    public float spawnInterval = 0.2f;

    [Tooltip("장판 지속 시간 (초)")]
    public float trailDuration = 2.0f;

    [Tooltip("장판 데미지 배율 (오브 기본 데미지의 몇 %인가)")]
    public float trailDamageScaling = 0.5f;
}
