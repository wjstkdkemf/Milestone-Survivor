using UnityEngine;

[CreateAssetMenu(fileName = "New Barrier Data", menuName = "Weapon Data/Mage/Fire Mage/Elemental/Barrier Skill")]
public class BarrierDataSO : WeaponDataSO
{
    [Header("Barrier Stats")]
    [Tooltip("피격되지 않고 버텨야 하는 시간 (초)")]
    public float chargeTime = 5.0f;
    
    [Tooltip("배리어 파괴 시 발생하는 넉백 범위")]
    public float knockbackRadius = 3.0f;
    
    [Tooltip("적을 밀어내는 힘")]
    public float knockbackForce = 10.0f;

    [Header("Visuals")]
    [Tooltip("플레이어 주변에 생성될 배리어 프리팹")]
    public GameObject barrierPrefab; 
    [Tooltip("배리어 파괴(넉백) 시 생성될 이펙트 (선택사항)")]
    public GameObject explosionEffectPrefab;
    
    [Header("Settings")]
    public LayerMask enemyLayerMask; // 적 감지용
    public LayerMask projectileLayerMask; // 적 투사체 감지용 (있다면)
}