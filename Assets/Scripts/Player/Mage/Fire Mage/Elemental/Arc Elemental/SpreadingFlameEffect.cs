using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadingFlameEffect : MonoBehaviour
{
    private static readonly HashSet<Enemy> burningEnemies = new HashSet<Enemy>();

    private readonly List<int> spreadResults = new List<int>(64);
    private readonly List<Enemy> spreadCandidates = new List<Enemy>(64);

    private Enemy target;
    private float damage;
    private float duration;
    private float tickInterval;
    private float spreadRadius;
    private float spreadDelay;
    private float remainingTime;
    private float tickTimer;
    private Vector3 lastTargetPosition;
    private string weaponID;
    private bool isActive;
    private Coroutine spreadRoutine;

    public static bool IsBurning(Enemy enemy)
    {
        return enemy != null && burningEnemies.Contains(enemy);
    }

    public void Attach(
        Enemy newTarget,
        float damagePerTick,
        float newDuration,
        float newTickInterval,
        float newSpreadRadius,
        float newSpreadDelay,
        string newWeaponID
    )
    {
        DetachCurrentTarget();

        target = newTarget;
        damage = damagePerTick;
        duration = newDuration;
        tickInterval = newTickInterval;
        spreadRadius = newSpreadRadius;
        spreadDelay = newSpreadDelay;
        weaponID = newWeaponID;
        remainingTime = duration;
        tickTimer = tickInterval;
        isActive = target != null;

        if (target != null)
        {
            burningEnemies.Add(target);
            lastTargetPosition = target.transform.position;
            transform.position = lastTargetPosition;
        }

        if (!isActive)
            ReturnToPool();
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (target == null || target.currentNormalState == Enemy.EnemyState.Dead || !target.gameObject.activeInHierarchy)
        {
            TrySpreadFromLastPosition();
            return;
        }

        lastTargetPosition = target.transform.position;
        transform.position = lastTargetPosition;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            ReturnToPool();
            return;
        }

        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            DealTickDamage();
            tickTimer = tickInterval;
        }
    }

    private void DealTickDamage()
    {
        if (target == null || target.currentNormalState == Enemy.EnemyState.Dead)
            return;

        target.TakeDamage(damage);

        if (RunStatisticsManager.Instance != null)
            RunStatisticsManager.Instance.RecordWeaponDamage(weaponID, (long)damage);

        if (target == null || target.currentNormalState == Enemy.EnemyState.Dead || !target.gameObject.activeInHierarchy)
            TrySpreadFromLastPosition();
    }

    private void TrySpreadFromLastPosition()
    {
        DetachCurrentTarget();

        if (spreadRoutine != null)
            return;

        if (spreadDelay > 0f)
            spreadRoutine = StartCoroutine(SpreadAfterDelay());
        else
            SpreadOrReturn();
    }

    private IEnumerator SpreadAfterDelay()
    {
        yield return new WaitForSeconds(spreadDelay);
        spreadRoutine = null;
        SpreadOrReturn();
    }

    private void SpreadOrReturn()
    {
        Enemy nextTarget = FindRandomSpreadTarget(lastTargetPosition);
        if (nextTarget == null)
        {
            ReturnToPool();
            return;
        }

        Attach(nextTarget, damage, duration, tickInterval, spreadRadius, spreadDelay, weaponID);
    }

    private Enemy FindRandomSpreadTarget(Vector3 center)
    {
        if (EnemySwarmSystem.Instance == null)
            return null;

        EnemySwarmSystem.Instance.GetEnemiesInRadius(center, spreadRadius, spreadResults);
        spreadCandidates.Clear();

        for (int i = 0; i < spreadResults.Count; i++)
        {
            Enemy enemy = EnemySwarmSystem.Instance.GetEnemyByIndex(spreadResults[i]);
            if (enemy == null || enemy.currentNormalState == Enemy.EnemyState.Dead)
                continue;

            if (IsBurning(enemy))
                continue;

            spreadCandidates.Add(enemy);
        }

        if (spreadCandidates.Count == 0)
            return null;

        return spreadCandidates[Random.Range(0, spreadCandidates.Count)];
    }

    private void DetachCurrentTarget()
    {
        if (target != null)
            burningEnemies.Remove(target);

        target = null;
    }

    private void ReturnToPool()
    {
        isActive = false;
        DetachCurrentTarget();

        if (spreadRoutine != null)
        {
            StopCoroutine(spreadRoutine);
            spreadRoutine = null;
        }

        if (ObjectPoolingManager.Instance != null)
            ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        isActive = false;
        DetachCurrentTarget();

        if (spreadRoutine != null)
        {
            StopCoroutine(spreadRoutine);
            spreadRoutine = null;
        }
    }
}
