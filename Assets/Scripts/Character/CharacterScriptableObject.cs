using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character")]
public class CharacterScriptableObject : ScriptableObject
{
    [Header("Visuals")] // 보기 좋게 헤더 추가
    public Sprite IconSprite;
    public RuntimeAnimatorController animatorController; // [추천] 배열 대신 여기에 직접 넣으세요!
    public GameObject CharacterPrefab; // 모델링을 통째로 바꾼다면
    [Header("Info")]
    public string CharacterName;
    public string description;
    public int Id;
    [Header("Starting Gear")]
    // [핵심] 이 캐릭터가 시작할 때 들고 시작할 무기 데이터!
    public UpgradeScriptableObject startingWeapon; 
    public List<UpgradeScriptableObject> StartingDeck;
    // 만약 무기를 여러 개 들고 시작한다면: public List<WeaponDataSO> startingWeapons;
    [Header("Stats")]
    public float BaseHP;
    public float Damage;
    public float MovementSpeed;
    public float Armor;
    public float HealthRegeneration;
    public float LuckBoost;
    public float CooldownReduction;
    public float DobleDamageChance;
    public List<StatModifier> statModifiers;

    public int costPerLevel; // Base cost per level
    public bool purchased;

    [Header("무기 이미지 정보")]
    public Vector3 weaponLocalPosition;
    public Vector2 weaponLocalDirection;
    public float weaponRotationOffset;
    public Sprite WeaponSprite;
    public WeaponVisualMode WeaponVisualMode;
}
