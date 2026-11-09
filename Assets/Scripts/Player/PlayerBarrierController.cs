using System.Collections.Generic;
using UnityEngine;

public class PlayerBarrierController : MonoBehaviour
{
    private readonly List<BarrierWeapon> barriers = new List<BarrierWeapon>();

    public void RegisterBarrier(BarrierWeapon barrier)
    {
        if (barrier == null || !barrier.isActiveAndEnabled)
            return;

        if (barriers.Contains(barrier))
            return;

        barriers.Add(barrier);
    }

    public void UnregisterBarrier(BarrierWeapon barrier)
    {
        barriers.Remove(barrier);
    }

    public bool TryBlockDamage()
    {
        BarrierWeapon selected = null;
        float shortestCooldown = float.PositiveInfinity;

        for (int i = 0; i < barriers.Count; i++)
        {
            BarrierWeapon barrier = barriers[i];

            if (barrier == null)
            {
                barriers.RemoveAt(i);
                i--;
                continue;
            }

            if (!barrier.CanBlockDamage)
                continue;

            float cooldown = barrier.RechargeDuration;

            if (selected == null || cooldown < shortestCooldown)
            {
                selected = barrier;
                shortestCooldown = cooldown;
            }
        }

        return selected != null && selected.TryBlockDamage();
    }
}