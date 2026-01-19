using UnityEngine;

[CreateAssetMenu(fileName = "New Chain Lightning Data", menuName = "Weapon Data/Mage/Chain Lightning Weapon")]
public class ChainLightningDataSO : WeaponDataSO
{
    [Header("Chain Lightning Specific Stats")]
    [Tooltip("첫 타겟 감지 사거리")]
    public float initialRange = 8f;
    [Tooltip("투사체 속도")]
    public float projectileSpeed = 15f;

    [Header("Chain Settings")]
    [Tooltip("최대 튕기는 횟수 (0이면 안 튕김)")]
    public int chainCount = 2;
    [Tooltip("다음 타겟을 찾는 범위 (팅기는 사거리)")]
    public float chainRange = 5f;
    [Tooltip("팅길 때마다 데미지 감소율 (1.0 = 변화 없음, 0.8 = 80% 데미지)")]
    public float damageReductionPerBounce = 0.9f;

    [Header("Damage Settings")]
    [Tooltip("플레이어 스탯 반영 비율")]
    public float playerDamageScaling = 0.3f;

    [Header("Setup")]
    [Tooltip("번개 투사체 프리팹 (DoDamage와 ChainLightningProjectile 스크립트 필요)")]
    public GameObject projectilePrefab;
    public LayerMask enemyLayerMask;
}