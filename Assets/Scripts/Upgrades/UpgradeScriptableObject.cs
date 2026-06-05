using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "NewUpgrade", menuName = "UpgradeObject")]

public class UpgradeScriptableObject : ScriptableObject
{
    [Header("UI Info")]
    public Sprite Icon;
    public string Title;
    [TextArea] public string Description; // TextArea를 쓰면 인스펙터에서 줄바꿈 가능

    [Header("Grade")]
    public UpgradeGrade Grade;

    [Header("Level System")]
    public int Points;     // 현재 레벨 (0부터 시작)
    public int MaxPoints;  // 최대 레벨 (만렙)
    
    // 레벨별 상세 설명 (1렙일 때 설명, 2렙일 때 설명...)
    // 기존에 쓰시던 방식 그대로 유지하면 좋습니다.
    public List<UpgradeLevelInfo> UpgradesList = new List<UpgradeLevelInfo>(); 

    [Header("Probability")]
    [Range(0, 100)]
    public int Chance;
    [Range(0, 100)]
    public int InitialChance;

    [Header("Type Settings")]
    // [핵심 1] 무기라면 여기에 데이터를 넣습니다. (없으면 스탯 업그레이드로 취급)
    public WeaponDataSO linkedWeaponData;

    // [핵심 2] 무기가 아니라면, 어떤 스탯을 올려줄지 결정합니다.
    public UpgradeType upgradeType;

    // [핵심 3] 스탯을 얼마나 올려줄지 (예: 체력 +10, 속도 +5)
    public float statValue; 

    public UpgradeLevelInfo GetLevelInfoOrNull()
    {
        if (UpgradesList == null || UpgradesList.Count == 0)
            return null;

        int index = Mathf.Clamp(Points, 0, UpgradesList.Count - 1);

        return UpgradesList[index];
    }

    public string GetCurrentShortDescription()
    {
        UpgradeLevelInfo info = GetLevelInfoOrNull();

        if (info == null || string.IsNullOrEmpty(info.ShortDescription))
            return Title;

        return info.ShortDescription;
    }

    public string GetCurrentDescription()
    {
        UpgradeLevelInfo info = GetLevelInfoOrNull();

        if (info == null || string.IsNullOrEmpty(info.Description))
            return Description;

        return info.Description;
    }

    // 깔끔해진 Enum (구체적인 무기 이름은 다 삭제!)
    public enum UpgradeType
    {
        Weapon,             // 무기 (linkedWeaponData가 있을 때)
        Stat_MaxHealth,     // 체력 증가
        Stat_Heal,          // 체력 회복
        Stat_MoveSpeed,     // 이동 속도
        Stat_Might,         // 공격력(Damage)
        Stat_Area,          // 범위
        Stat_Speed,         // 투사체 속도
        Stat_Duration,      // 지속 시간
        Stat_Amount,        // 투사체 개수 (ExtraBullet)
        Stat_Cooldown,      // 쿨타임 감소
        Stat_Luck,          // 행운
        Stat_Greed,         // 획득 반경/골드 등
        Stat_Growth         // 경험치 보너스
    }
}

// 클래스 이름이 Upgrade면 헷갈리니까 조금 더 명확하게 변경 추천
[System.Serializable]
public class UpgradeLevelInfo 
{
    public string ShortDescription;       // 레벨별 아이콘이 다르다면 사용
    public string Description; // "데미지 +5 증가" 같은 레벨별 텍스트
    // public float Value; // <- 이 값은 위쪽 statValue나 Weapon 자체 레벨업 로직으로 대체 가능하므로 삭제 고려
}
public enum UpgradeGrade
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
