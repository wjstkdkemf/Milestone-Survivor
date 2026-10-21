using UnityEngine.AI;
using UnityEngine;

public abstract class NavEnemy : Enemy
{
    protected NavMeshAgent agent;
    public override bool RequiresNavMesh => true;

    protected override void OnEnable()
    {
        useSwarmMovement = false;

        base.OnEnable();

        TryGetComponent<NavMeshAgent>(out var ag);

        if(ag != null)
        {
            agent = ag;

            agent.enabled = true; 
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.updatePosition = false;
            agent.speed = speed;
            agent.stoppingDistance = attackRange;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (agent != null && agent.enabled) agent.enabled = false;
    }
    public override void ManualUpdate()
    {
        if (currentNormalState == EnemyState.Dead || player == null || stopMoving) return;

        UpdateFlash();
        CheckStatusEffects();

        knockBackTime -= Time.deltaTime;
        coolDownTimer -= Time.deltaTime;

        float distanceSqrToPlayer = (player.position - transform.position).sqrMagnitude;
        HandleDistanceCheck(distanceSqrToPlayer);

        if (agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
        {
            RequestReposition(RepositionReason.OffNavMesh);
            return;
        }

        DetermineState(distanceSqrToPlayer);
    
         Vector3 delta = player.position - transform.position;
         UpdateFacingDirection(delta);
         
         if (knockBackTime <= 0)
         {
             HandleNavMovement();
         }
         else if (!CantBeKnocked)
         {
             ApplyKnockbackEffect();
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
    private void ApplyKnockbackEffect()
    {
        if (agent != null && agent.isActiveAndEnabled) agent.isStopped = true;
        
        MoveKnockback();
        
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
    public override void ApplyReposition(Vector3 position)
    {
        if (agent == null ||
            !agent.enabled ||
            !agent.Warp(position))
        {
            FinishReposition(false);
            return;
        }

        transform.position = position;
        agent.nextPosition = position;

        if (player != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.SetDestination(player.position);
        }

        FinishReposition(true);
    }

    public void OnNavMeshUpdated()
    {
        if (currentNormalState == EnemyState.Dead || stopMoving) return;

        if (agent != null && agent.enabled)
        {
            if (!agent.isOnNavMesh)
            {
                RequestReposition(RepositionReason.MapChanged);
            }
            else
            {
                agent.ResetPath();
                if (player != null) agent.SetDestination(player.position);
            }
        }
    }
    protected override void CheckStatusEffects()
    {
        if (isSlowed)
        {
            // 아직 만료 시간이 안 지났다면? 속도를 깎아버립니다.
            if (Time.time < slowEndTime)
            {
                agent.speed = baseSpeed * (1f - currentSlowPercent);
            }
            // 시간이 지났다면? 원상 복구! (Exit 이벤트나 코루틴 종료 대기 필요 없음)
            else
            {
                ResetStatusEffectsByNav();
            }
        }
    }
    private void ResetStatusEffectsByNav()
    {
        isSlowed = false;
        agent.speed = baseSpeed;
        currentStateColor = defaultColor;

        if (spriteRenderer != null) spriteRenderer.color = currentStateColor;
    }
}
