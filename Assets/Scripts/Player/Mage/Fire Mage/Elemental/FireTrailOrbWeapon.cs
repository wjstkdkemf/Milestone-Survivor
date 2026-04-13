using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrailOrbWeapon : OrbWeapon
{
    private GameObject trailPrefab;
    private float spawnInterval;
    private float trailDuration;
    private float trailDamageScaling;

    public override void Initialize(WeaponDataSO data)
    {
        base.Initialize(data); 

        if (data is FireTrailOrbDataSO fireData)
        {
            trailPrefab = fireData.trailPrefab;
            spawnInterval = fireData.spawnInterval;
            trailDuration = fireData.trailDuration;
            trailDamageScaling = fireData.trailDamageScaling;
        }
        
        UpdateTrailInfoForExistingOrbs();
    }

    protected override void SetupSpawnedOrb(GameObject orb)
    {
        base.SetupSpawnedOrb(orb);

        if (orb.TryGetComponent<FireTrailOrb>(out var fireOrbScript))
        {
            float finalTrailDamage = GetDamage() * trailDamageScaling;
            fireOrbScript.SetTrailInfo(trailPrefab, spawnInterval, trailDuration, finalTrailDamage);
        }
    }

    private void UpdateTrailInfoForExistingOrbs()
    {
        float finalTrailDamage = GetDamage() * trailDamageScaling;
        foreach (var orbObj in spawnedOrbs)
        {
            if (orbObj != null && orbObj.TryGetComponent<FireTrailOrb>(out var fireOrbScript))
            {
                fireOrbScript.SetTrailInfo(trailPrefab, spawnInterval, trailDuration, finalTrailDamage);
            }
        }
    }

    public override void UpgradeDamage(float amount)
    {
        base.UpgradeDamage(amount);
        UpdateTrailInfoForExistingOrbs();
    }
}
