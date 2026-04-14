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

        useSwarmMovement = false;

        if (agent == null) agent = GetComponent<NavMeshAgent>();

        agent.enabled = true; 
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.updatePosition = false;
        agent.speed = speed;
        agent.stoppingDistance = attackRange;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (agent != null && agent.enabled) agent.enabled = false;
    }
    public override void ManualUpdate()
    {
        if (currentNormalState == EnemyState.Dead || player == null || stopMoving) return;

        UpdateFlash();

        knockBackTime -= Time.deltaTime;
        coolDownTimer -= Time.deltaTime;

        float distanceSqrToPlayer = (player.position - transform.position).sqrMagnitude;
        DetermineState(distanceSqrToPlayer);
    
         Vector3 delta = player.position - transform.position;
         UpdateFacingDirection(delta);
         
         if (knockBackTime <= 0)
         {
             HandleNavMovement();
         }
         else if (!CantBeKnocked)
         {
             ApplyKnockbackEffect(delta);
         }
    
         HandleAction(delta);
     }
    protected virtual void HandleNavMovement()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        switch (currentNormalState)
        {
            case EnemyState.Chasing:
                agent.isStopped = false;
                agent.SetDestination(player.position);
                break;

            case EnemyState.Attacking:
                break;
            case EnemyState.Idle:
                agent.isStopped = true;
                break;
        }
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            transform.position += agent.velocity * Time.deltaTime;
        }

        // Agent 위치를 오브젝트 위치와 동기화 (updatePosition = false일 때 필수)
        agent.nextPosition = transform.position;
    }
    private void ApplyKnockbackEffect(Vector3 delta)
    {
        if (agent != null && agent.isActiveAndEnabled) agent.isStopped = true;
        
        float finalKnockBack = knockBackForce * (1 + (PlayerStats.Instance != null ? PlayerStats.Instance.KnockBackBonus : 0));
        Vector3 knockbackDir = -delta.normalized;
        transform.position += knockbackDir * finalKnockBack * Time.deltaTime;
        
        if (agent != null) agent.nextPosition = transform.position;
    }
    protected override void HandleAction(Vector3 delta)
    {
        if(currentNormalState == EnemyState.Attacking && coolDownTimer <= 0)
        {
            Attack();
            coolDownTimer = coolDown;
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