using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinArcherScript : Enemy
{
    // Start is called before the first frame update
    private Animator animator;

    void Start()
    {
        attackRange = 5.0f;
        animator = GetComponent<Animator>();
    }

    public override void Attack()
    {
        // Example melee attack logic
        animator.SetTrigger("Attack");
        Debug.Log(gameObject.name + " performs a GoblinArcher attack, dealing " + damage + " damage.");
    }
}
