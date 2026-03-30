using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewStage", menuName = "Stage")]
public class StageScriptableObject : ScriptableObject
{
    public Sprite IconSprite;
    public string StageName; // Display name
    public int SceneIndex;
    public string description; // Tooltip or UI description

    public float TimeLimit;
    public float ClockSpeed;
    public float HPBoost;
    public float DamageBoost;
    public float MovementSpeed;
    public float LuckBoost;
    public float XPBoost;

    public bool IsUnlocked;

    public string GetStatsAsString()
    {
        return $"TimeLimit: {TimeLimit}, ClockSpeed: {ClockSpeed}, HPBoost: {HPBoost}, DamageBoost: {DamageBoost}, MovementSpeed: {MovementSpeed}, LuckBoost: {LuckBoost}, XPBoost: {XPBoost}";
    }
}
