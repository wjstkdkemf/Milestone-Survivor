using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static LTGUI;

public class PetController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackRange;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float baseDamage;
    [SerializeField] private float chanceDoubleDamage;
    private float lastAttackTime = 0;
    [SerializeField] private float Distance=7;
    private Transform player; // Reference to the player
    public float maxDistanceFromPlayer = 7f; // Maximum distance the pet can be from the player
    private GameObject closestEnemy;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        GetComponent<DoDamage>().damage = baseDamage;
    }
    private void FixedUpdate()
    {
        MoveAndAttack();
    }
    // Move and attack enemies
    public void MoveAndAttack()
    {
        // First, ensure the player exists before doing anything.
        if (player == null)
        {
            // If player is not found (e.g., in a non-game scene), do nothing.
            return;
        }

        // Check if the pet is too far from the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > maxDistanceFromPlayer)
        {
            // Move closer to the player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            transform.position += directionToPlayer * moveSpeed * Time.deltaTime;
        }
        else
        {
            FindClosestEnemy();

            // If an enemy was found
            if (closestEnemy != null)
            {
                Collider2D nearestEnemy = closestEnemy.GetComponent<Collider2D>();
                if (nearestEnemy != null)
                {
                    Vector3 direction = (nearestEnemy.transform.position - transform.position).normalized;
                    float distanceToEnemy = Vector3.Distance(transform.position, nearestEnemy.transform.position);

                    // Move towards the enemy if out of range
                    if (distanceToEnemy > attackRange)
                    {
                        transform.position += direction * moveSpeed * Time.deltaTime;
                    }

                    // Attack if in range and cooldown has passed
                    if (Time.time > lastAttackTime + attackCooldown)
                    {
                        MeleeAttack(nearestEnemy);
                        lastAttackTime = Time.time;
                    }
                }
            }
            // If no enemy is found (closestEnemy is null), the pet will simply do nothing (stay near the player).
        }
    }

    // Find the nearest enemy within range
    /*  Collider2D FindNearestEnemy()
      {
          Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, enemyLayer);
          Collider2D nearestEnemy = null;
          float closestDistance = Mathf.Infinity;

          foreach (Collider2D enemy in enemies)
          {
              float distance = Vector3.Distance(transform.position, enemy.transform.position);
              if (distance < closestDistance && nearestEnemy.GetComponent<Enemy>().IsActived)
              {
                  closestDistance = distance;
                  nearestEnemy = enemy;
              }
          }

          return nearestEnemy;
      }*/
    void FindClosestEnemy()
    {
        Enemy[] enemys = GameObject.FindObjectsOfType<Enemy>();
        Enemy[] allEnemies = enemys;
        closestEnemy = null;
        foreach (Enemy currentEnemy in allEnemies)
        {
            float distanceToEnemy = (currentEnemy.transform.position - this.transform.position).sqrMagnitude;
            if (distanceToEnemy < Distance * 100 && currentEnemy.IsActived)
            {

                closestEnemy = currentEnemy.gameObject;
            }
        }
    }

    // Method for melee attack (you can add animations or effects here)
    void MeleeAttack(Collider2D enemy)
    {
        bool isDoubleDamage = Random.Range(0f, 100f) < chanceDoubleDamage;
        // float currentDamage = PlayerStats.Instance.;
        // Logic for melee damage (you can integrate with enemy health system)
        Debug.Log("Melee attack on " + enemy.name);
    }
    void OnDrawGizmosSelected()
    {
        // Visualize attack range and max distance from player
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
    }
}