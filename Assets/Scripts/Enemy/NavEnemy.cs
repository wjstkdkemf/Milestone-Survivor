using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public abstract class NavEnemy : Enemy
{
    protected NavMeshAgent agent;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.updatePosition = false;

        agent.enabled = true; 
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (agent != null && agent.enabled) agent.enabled = false;
    }

    protected override void HandleMovement(float distanceSqrToPlayer, Vector3 delta)
    {
        if (knockBackTime > 0 && !CantBeKnocked) return;

        switch (currentNormalState)
        {
            case EnemyState.Chasing:
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    agent.SetDestination(player.position);

                    transform.position += (Vector3)agent.velocity * Time.deltaTime;
                }
                break;

            case EnemyState.Attacking:
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                }
                
                if (coolDownTimer <= 0)
                {
                    Attack();
                    coolDownTimer = coolDown;
                }
                break;
                
            case EnemyState.Idle:
                 if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.ResetPath();
                 break;
        }

        if (agent.isActiveAndEnabled)
        {
            if (Vector3.Distance(transform.position, agent.nextPosition) > 1.0f)
            {
                agent.nextPosition = transform.position;
            }
        }
    }

    protected override void RepositionEnemy()
    {
        Vector2 randomPoint = Random.insideUnitCircle.normalized * respawnRadius;
        Vector3 potentialPos = player.position + new Vector3(randomPoint.x, randomPoint.y, 0);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(potentialPos, out hit, 3.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            transform.position = hit.position;
            
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
            }
        }
    }
    public void OnNavMeshUpdated()
    {
        if (currentNormalState == EnemyState.Dead || stopMoving) return;

        if (agent != null && agent.enabled)
        {
            if (!agent.isOnNavMesh)
            {
                RepositionEnemy(); 
            }
            else
            {
                agent.ResetPath();
                if (player != null) agent.SetDestination(player.position);
            }
        }
    }
}