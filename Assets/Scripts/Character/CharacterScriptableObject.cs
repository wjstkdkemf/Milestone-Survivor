using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character")]
public class CharacterScriptableObject : ScriptableObject
{
    [Header("Visuals")] // 보기 좋게 헤더 추가
    public Sprite IconSprite;
    public RuntimeAnimatorController animatorController; // [추천] 배열 대신 여기에 직접 넣으세요!
    public GameObject CharacterPrefab; // 모델링을 통째로 바꾼다면
    [Header("Info")]
    public string CharacterName;//세이브 저장 분류용
    [SerializeField] private LocalizedString localizedCharacterName;
    [SerializeField] private LocalizedString localizedDescription;
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
    public List<StatModifier> statModifiers;// 레벨업 보정치

    public int costPerLevel; // Base cost per level
    public bool purchased;

    [Header("무기 이미지 정보")]
    public Vector3 weaponLocalPosition;
    public Vector2 weaponLocalDirection;
    public float weaponRotationOffset;
    public Sprite WeaponSprite;
    public WeaponVisualMode WeaponVisualMode;

    public string GetLocalizedName()
    {
        return GetLocalizedString(localizedCharacterName);
    }

    public string GetLocalizedDescription()
    {
        return GetLocalizedString(localizedDescription);
    }

    private static string GetLocalizedString(LocalizedString localizedString)
    {
        if (localizedString != null && !localizedString.IsEmpty)
        {
            string localized = localizedString.GetLocalizedString();

            if (!string.IsNullOrEmpty(localized))
                return localized;
        }

        return "";
    }
}

public static class CharacterLocalization
{
    public const string TableName = "Character_Table";

    public static string Get(string key, string fallback)
    {
        if (!string.IsNullOrEmpty(key))
        {
            string localized = LocalizationSettings.StringDatabase.GetLocalizedString(TableName, key);

            if (!string.IsNullOrEmpty(localized))
                return localized;
        }

        return fallback;
    }

    public static string GetStatLabel(string statName)
    {
        switch (statName)
        {
            case nameof(CharacterScriptableObject.BaseHP):
                return Get("character.stat.base_hp", "Base HP");
            case nameof(CharacterScriptableObject.Damage):
                return Get("character.stat.damage", "Damage");
            case nameof(CharacterScriptableObject.MovementSpeed):
                return Get("character.stat.movement_speed", "Movement Speed");
            case nameof(CharacterScriptableObject.Armor):
                return Get("character.stat.armor", "Armor");
            case nameof(CharacterScriptableObject.HealthRegeneration):
                return Get("character.stat.health_regeneration", "Health Regeneration");
            case nameof(CharacterScriptableObject.LuckBoost):
                return Get("character.stat.luck_boost", "Luck Boost");
            case nameof(CharacterScriptableObject.CooldownReduction):
                return Get("character.stat.cooldown_reduction", "Cooldown Reduction");
            case nameof(CharacterScriptableObject.DobleDamageChance):
                return Get("character.stat.double_damage_chance", "Double Damage Chance");
            case "MaxHealth":
                return Get("character.stat.base_hp", "Base HP");
            case "ProjectileSpeed":
                return Get("character.stat.projectile_speed", "Projectile Speed");
            case "KnockBack":
                return Get("character.stat.knockback", "Knockback");
            case "XPBoost":
                return Get("character.stat.growth", "Growth");
            default:
                return GetStatModifierLabel(statName);
        }
    }

    public static string GetStatModifierLabel(string statName)
    {
        switch (statName)
        {
            case "Damage":
                return Get("character.stat.damage", "Damage");
            case "MovementSpeed":
            case "MoveSpeed":
                return Get("character.stat.movement_speed", "Movement Speed");
            case "Armor":
                return Get("character.stat.armor", "Armor");
            case "HealthRegeneration":
                return Get("character.stat.health_regeneration", "Health Regeneration");
            case "LuckBoost":
            case "Luck":
                return Get("character.stat.luck_boost", "Luck Boost");
            case "CooldownReduction":
                return Get("character.stat.cooldown_reduction", "Cooldown Reduction");
            case "DobleDamageChance":
            case "DoubleDamageChance":
                return Get("character.stat.double_damage_chance", "Double Damage Chance");
            case "MaxHealth":
                return Get("character.stat.base_hp", "Base HP");
            case "ProjectileSpeed":
                return Get("character.stat.projectile_speed", "Projectile Speed");
            case "KnockBack":
                return Get("character.stat.knockback", "Knockback");
            case "XPBoost":
                return Get("character.stat.growth", "Growth");
            default:
                return statName;
        }
    }
}
