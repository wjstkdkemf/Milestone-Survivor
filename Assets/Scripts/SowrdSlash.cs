using UnityEngine;

public class SowrdSlash : AttackBase
{
    public int SlashCount;
    [SerializeField] private Transform[] slashPostion;
    [SerializeField] private GameObject slashPrefab;

    // The `cooldown` and `baseDamage` fields from AttackBase will be used
    // and can be set in the Unity Inspector.

    // The base class's Update method handles the cooldown timer.
    // We check for readiness in FixedUpdate, which is consistent with the original implementation.
    private void FixedUpdate()
    {
        if (IsReady())
        {
            PerformAttack();
        }
    }

    public override void PerformAttack()
    {
        // Reset cooldown immediately to prevent multiple attacks in the same frame.
        ResetCooldown();

        for (int i = 0; i < SlashCount; i++)
        {
            GameObject ob = Instantiate(slashPrefab, slashPostion[i].position, Quaternion.identity, slashPostion[i]);
            
            // Get the DoDamage component and assign the damage.
            DoDamage damageComponent = ob.GetComponent<DoDamage>();
            if (damageComponent != null)
            {
                // Use GetDamage() which returns baseDamage, as defined in the base class.
                damageComponent.damage = GetDamage();
            }
        }
    }
}