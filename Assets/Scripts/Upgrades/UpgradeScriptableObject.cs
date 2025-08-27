using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "NewUpgrade", menuName = "UpgradeObject")]

public class UpgradeScriptableObject : ScriptableObject
{

    public Sprite Icon;
    public string Title;
    public string Description;
    public int Points;
    public int MaxPoints;
    public List<Upgrade> UpgradesList = new List<Upgrade>();
    public enum UpgardeEnum { SpawnPet, ExtraBullet, AddHealth, Heal, AddSpeed, AddDamge, NewOrb , AttackSpeed , ShootProjectile, RandomExplosions, LightningBolt, ExperienceBonus, MeteorShower, AstralBarrier, StarlightInfusion, AcornBarrage, ForestGuardianForm, EntanglingVines, KnifeProjectile, ExperienceBoost, SwordSlash };
    public UpgardeEnum Upgarde ;
    [Range(0,100)]
    public int Chance;

}

[System.Serializable]
public class Upgrade
{
    public Sprite Icon;
    public string Title;
    public string Description;
    public float Value;

}
