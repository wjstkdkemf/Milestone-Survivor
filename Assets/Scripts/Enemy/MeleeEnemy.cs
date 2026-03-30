using UnityEngine;

public class MeleeEnemy : Enemy
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public override void Attack()
    {
        // Example melee attack logic
        animator.SetTrigger("Attack");
        playerHealth.TakeDamage(damage);
        
        Debug.Log(gameObject.name + " performs a melee attack, dealing " + damage + " damage.");
    }
}