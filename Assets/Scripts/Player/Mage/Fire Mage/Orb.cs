using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : ZoneDamageArea
{
    private float chanceDoubleDamage;

    public void SetInfo(float _baseDamage, float _chanceDoubleDamage)

    {

        damageComponent.damage = _baseDamage;

        chanceDoubleDamage= _chanceDoubleDamage;

    }

}
