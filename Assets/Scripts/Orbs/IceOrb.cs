using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LTGUI;

public class IceOrb : ZoneDamageArea
{
    private float chanceDoubleDamage;
    public void SetInfo(float _baseDamage, float _chanceDoubleDamage)
    {
        damageComponent.damage = _baseDamage;
        chanceDoubleDamage= _chanceDoubleDamage;
    }
}
