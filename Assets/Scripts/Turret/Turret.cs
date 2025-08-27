using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    float nextFire;
    public int bulletNumber =2;
    public int BulletDamage = 2;
    [SerializeField] private float fireRate;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float Distance;
    private Transform closestEnemyPostion;
    void Update()
    {
        FindClosestEnemy();
        Fire();
    }




    void Fire()
    {

        if (Time.time > nextFire && closestEnemyPostion != null&& bulletNumber>0)
        {
            for (int i = 0; i < bulletNumber; i++)
            {
                StartCoroutine(ShootBullet(i*.1f));
            }
            
            nextFire = Time.time + fireRate;

        }

    }
    void FindClosestEnemy()
    {
        // Find all MonoBehaviours in the scene
        MonoBehaviour[] allBehaviours = GameObject.FindObjectsOfType<MonoBehaviour>();
        List<IDamageable> allEnemies = new List<IDamageable>();

        // Filter out objects that implement IDamageable and are on the Enemy layer
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        foreach (var behaviour in allBehaviours)
        {
            if (behaviour is IDamageable damageable && behaviour.gameObject.layer == enemyLayer)
            {
                allEnemies.Add(damageable);
            }
        }

        Transform closestEnemyTransform = null;
        float closestDistanceSqr = Mathf.Infinity;

        // Loop through all enemies to find the closest one
        foreach (IDamageable currentEnemy in allEnemies)
        {
            if (currentEnemy != null && currentEnemy is MonoBehaviour monoEnemy) // Ensure the enemy is valid
            {
                float distanceToEnemySqr = (monoEnemy.transform.position - this.transform.position).sqrMagnitude;

                if (distanceToEnemySqr < closestDistanceSqr && distanceToEnemySqr < Distance * Distance) // Check distance
                {
                    closestDistanceSqr = distanceToEnemySqr;
                    closestEnemyTransform = monoEnemy.transform;
                }
            }
        }

        closestEnemyPostion = closestEnemyTransform;
    }


    IEnumerator ShootBullet(float delay)
    {
        yield return new WaitForSeconds(delay);

      //  AudioManager.instance.PlaySound("Spell");
        //  GameObject Bullett = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        GameObject Bullett = ObjectPoolingManager.instance.spawnGameObject(bulletPrefab,transform.position,Quaternion.identity);
        Bullett.GetComponent<TurretBullet>().EnemyPosition = closestEnemyPostion;
        Bullett.GetComponent<DoDamage>().damage = BulletDamage;

    }
}

    