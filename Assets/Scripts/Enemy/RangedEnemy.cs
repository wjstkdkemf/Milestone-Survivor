using UnityEngine;

public class RangedEnemy : Enemy
{
    public GameObject projectilePrefab; // The projectile to shoot
    public Transform firepoint; // The point from where the projectile will be fired
    public float projectileSpeed = 5f; // Speed of the projectile
    public int projectileCount = 3; // Number of projectiles to shoot
    public float spreadAngle = 15f; // Spread angle for shotgun effect

    public override void Attack()
    {
        var poolManager = ObjectPoolingManager.Instance;
        if (poolManager == null || projectilePrefab == null ||
            firepoint == null || player == null)
            return;

        Vector2 direction = player.position - firepoint.position;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        for (int i = 0; i < projectileCount; i++)
        {
            float offset = spreadAngle *
                ((i - (projectileCount - 1) / 2f) /
                Mathf.Max(1, projectileCount - 1));
            float angle = baseAngle + offset;

            GameObject bullet = poolManager.spawnGameObject(
                projectilePrefab, firepoint.position,
                Quaternion.Euler(0f, 0f, angle), PoolScope.Theme);

            if (bullet == null) return;
            if (!bullet.TryGetComponent<Rigidbody2D>(out var rb))
            {
                Debug.LogError("Projectile prefab requires Rigidbody2D.", projectilePrefab);
                poolManager.ReturnObjectToPool(bullet);
                return;
            }

            float radians = angle * Mathf.Deg2Rad;
            rb.angularVelocity = 0f;
            rb.velocity = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians))
                * projectileSpeed;
        }
    }
}