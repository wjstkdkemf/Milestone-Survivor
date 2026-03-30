using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEncounter : MapMaker
{
    public void BossEncount()
    {
        StartCoroutine(CoBossEncounter());
    }
    IEnumerator CoBossEncounter()
    {
        enCounterSystem.EnterMap(this);
        yield return null;

        enCounterSystem.BossEncount();
    }
}
