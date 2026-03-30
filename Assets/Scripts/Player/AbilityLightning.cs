using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityLightning : MonoBehaviour
{
    float timer;
    public int LightningNumber = 2;
    public int LightningDamage = 2;
    public float damage=10;
    [SerializeField] private float fireRate;
    [SerializeField] private GameObject LightningPrefab;
    [SerializeField] private float Distance;
    public List<GameObject> closeEnemyPostions;
    void Start()
    {
        timer = fireRate;
    }

    void FixedUpdate()
    {
        if (timer <= 0 && LightningNumber > 0)
        {
            for (int i = 0; i < LightningNumber; i++)
            {

                StartCoroutine(ShootBullet(i * .1f));
            }


            timer = fireRate;
        }
        timer -= Time.deltaTime;
    }
    void FindClosestEnemy()
    {
        Enemy[] enemys = GameObject.FindObjectsOfType<Enemy>();
        Enemy[] allEnemies = enemys;
        closeEnemyPostions.Clear();
        foreach (Enemy currentEnemy in allEnemies)
        {
            float distanceToEnemy = (currentEnemy.transform.position - this.transform.position).sqrMagnitude;
            if (distanceToEnemy < Distance * 10 && currentEnemy.IsActived)
            {

                closeEnemyPostions.Add(currentEnemy.gameObject);
            }
        }
    }


    IEnumerator ShootBullet(float delay)
    {
        yield return new WaitForSeconds(delay);
        FindClosestEnemy();
        if (closeEnemyPostions.Count > 0)
        {


            int random = Random.Range(0, closeEnemyPostions.Count);

            Vector2 postion = new Vector3(closeEnemyPostions[random].transform.position.x, closeEnemyPostions[random].transform.position.y + .5f);
            GameObject Bullett = Instantiate(LightningPrefab, postion, Quaternion.identity);
            Bullett.GetComponent<DoDamage>().damage = damage;
        }

    }
}
