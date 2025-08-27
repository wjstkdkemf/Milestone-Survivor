using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character")]
public class CharacterScriptableObject : ScriptableObject
{
    public Sprite IconSprite;
    public string CharacterName; // Display name
    public string description; // Tooltip or UI description
    public int Id;
    public float BaseHP;
    public float Damage;
    public float MovementSpeed;
    public float Armor;
    public float HealthRegeneration;
    public float LuckBoost;
    public float CooldownReduction;
    public float DobleDamageChance;
    public float XPBoost; 

    public int costPerLevel; // Base cost per level
    public bool purchased;
    public GameObject CharacterPrefab;
}
