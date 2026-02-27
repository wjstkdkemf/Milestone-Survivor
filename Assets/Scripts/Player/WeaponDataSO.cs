using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("기본 정보")]
    //public string weaponName; // 무기 이름 (예: "마늘")
    //public Sprite icon;       // 무기 아이콘
    //public string description; // 설명

    [Header("필수 연결")]
    // 실제 무기 로직이 들어있는 프리팹 (WeaponBase가 붙어있는 프리팹)
    public GameObject weaponPrefab;
    public UpgradeScriptableObject upgradeData;

    [Header("밸런스 데이터 (선택사항)")]
    public float baseDamage;
    public float baseCooldown;
    [Header("융합 스킬 정보")]
    public List<WeaponDataSO> fusionWeaponData;
    // 필요한 데이터들을 여기에 추가하면 기획자가 밸런스 잡기 좋습니다.
}
