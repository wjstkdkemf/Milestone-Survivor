using UnityEngine;

public class MeleeEnemy : Enemy
{
    public override void Attack()
    {
        // Example melee attack logic
        Debug.Log(gameObject.name + " performs a melee attack, dealing " + damage + " damage.");
    }
}