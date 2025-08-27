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
        // Calculate the direction to the player
        Vector2 directionToPlayer = (player.position - firepoint.position).normalized;

        // Base angle to the player
        float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        // Loop to shoot multiple projectiles
        for (int i = 0; i < projectileCount; i++)
        {
            // Calculate angle for each projectile
            float angleOffset = spreadAngle * ((i - (projectileCount - 1) / 2f) / Mathf.Max(1, (projectileCount - 1)));
            float finalAngle = baseAngle + angleOffset;

            // Rotate firepoint for this projectile
            Quaternion projectileRotation = Quaternion.Euler(0, 0, finalAngle);

            // Instantiate the projectile
            GameObject bullet = Instantiate(projectilePrefab, firepoint.position, projectileRotation);

            // Ensure the projectile has a Rigidbody2D
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("Projectile prefab is missing a Rigidbody2D component!");
                return;
            }

            // Calculate the final direction based on the angle
            Vector2 finalDirection = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad)).normalized;

            // Apply force to the projectile
            rb.velocity = finalDirection * projectileSpeed;
        }
    }
}