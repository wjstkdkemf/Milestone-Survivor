using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : SkillProjectileBase
{
    private float chanceDoubleDamage;

    public void SetInfo(float _baseDamage, float _hitRadius, float _chanceDoubleDamage)
    {
        damage = _baseDamage;
        hitRadius = _hitRadius; 
        maxHits = -1; 

        chanceDoubleDamage = _chanceDoubleDamage;
    }

    public override void OnHit(Enemy hitEnemy)
    {
    }

}
