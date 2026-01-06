using System.Collections;
using UnityEngine;

public class Turret : AttackBase
{
    [Header("Turret Specifics")]
    [SerializeField] private float targetUpdateRate = 0.5f;
    [SerializeField] public int bulletNumber = 2;
    [SerializeField] private float playerDamageScaling = 0.1f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float Distance;
    [SerializeField] private LayerMask enemyLayerMask;

    private float targetUpdateTimer;
    private Transform closestEnemyPosition;

    // The base class's Awake() will automatically handle getting the PlayerStats instance.
    // The `cooldown` field from AttackBase will be used for fire rate.
    // The `baseDamage` field from AttackBase will be used for base bullet damage.

    protected override void Update()
    {
        base.Update(); // Process cooldown timer from AttackBase

        // Targeting logic remains the same
        targetUpdateTimer -= Time.deltaTime;
        if (targetUpdateTimer <= 0f)
        {
            UpdateTarget();
            targetUpdateTimer = targetUpdateRate;
        }

        // Firing logic now uses the AttackBase cooldown system
        if (closestEnemyPosition != null && IsReady())
        {
            PerformAttack();
        }
    }

    public override void PerformAttack()
    {
        ResetCooldown(); // Use the base class method to reset cooldown

        for (int i = 0; i < bulletNumber; i++)
        {
            StartCoroutine(ShootBullet(i * 0.1f));
        }
    }

    public override float GetDamage()
    {
        // Custom damage calculation for the turret, including player's damage bonus
        if (playerStats == null) return baseDamage;
        return baseDamage + (playerStats.DamageBonus * playerDamageScaling);
    }

    IEnumerator ShootBullet(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject bullet = ObjectPoolingManager.instance.spawnGameObject(bulletPrefab, transform.position, Quaternion.identity);

        if (bullet.GetComponent<TurretBullet>() != null)
        {
            bullet.GetComponent<TurretBullet>().EnemyPosition = closestEnemyPosition;
        }
        
        if (bullet.GetComponent<DoDamage>() != null)
        {
            // Use the overridden GetDamage() to calculate final damage
            bullet.GetComponent<DoDamage>().damage = 0.5f;//GetDamage(); 
        }
    }

    void UpdateTarget()
    {
        // This targeting method remains unchanged
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, Distance, enemyLayerMask);
        
        Transform closestEnemyTransform = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.TryGetComponent<IDamageable>(out _))
            {
                Transform currentEnemy = hit.transform;
                float distanceToEnemySqr = (currentEnemy.position - this.transform.position).sqrMagnitude;

                if (distanceToEnemySqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceToEnemySqr;
                    closestEnemyTransform = currentEnemy;
                }
            }
        }
        closestEnemyPosition = closestEnemyTransform;
    }
}