using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Job", menuName = "Job Data")]
public class JobDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string jobName; // 예: Mage, Shooter
    public string description;
    public int jobTier; // 1차 , 2차 구분용

    [System.Serializable]
    public struct JobRequirement
    {
        public UpgradeScriptableObject requiredUpgrade; // 필요한 업그레이드 카드
        [Range(1, 10)]
        public int requiredLevel; // 요구 레벨 (이 레벨 '이상'이어야 함)
    }

    [Header("Requirements (전직 조건)")]
    // [핵심 변경 2] 기존 WeaponDataSO 리스트 대신 구조체 리스트 사용
    public List<JobRequirement> requirements; 
    
    // 혹은 특정 레벨 이상이어야 한다면
    // public int requiredLevel = 2; 
    [Header("분기점")]
    public List<JobDataSO> nextAbleJobs;
    [Header("Restrictions (삭제할 업그레이드)")]
    public List<UpgradeScriptableObject> bannedUpgrades;

    [Header("Benefits (전직 혜택)")]
    // 이 직업이 되면 확률이 올라갈 업그레이드 카드들
    public List<UpgradeScriptableObject> bonusUpgrades;
    public int bonusChanceAmount = 50; // 확률 증가량
}