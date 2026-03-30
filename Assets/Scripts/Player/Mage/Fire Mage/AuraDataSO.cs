using UnityEngine;

[CreateAssetMenu(fileName = "New Aura Data", menuName = "Weapon Data/Mage/Fire Mage/Aura Skill")]
public class AuraDataSO : WeaponDataSO
{
    [Header("Aura Settings (오라 설정)")]
    [Tooltip("오라 프리팹 (CircleCollider2D Trigger, ZoneDamageArea, DoDamage 포함)")]
    public GameObject auraPrefab;
    
    [Tooltip("기본 반경 (크기)")]
    public float baseRadius = 3f;
    
    [Tooltip("데미지 발생 주기")]
    public float tickRate = 0.5f;

    [Header("Effects (효과)")]
    [Tooltip("데미지를 줄 것인가?")]
    public bool applyDamage = true;
    
    [Tooltip("슬로우를 걸 것인가?")]
    public bool applySlow = true;
    
    [Tooltip("슬로우 비율 (예: 0.3 = 30% 느려짐)")]
    public float slowPercentage = 0.3f;
}
