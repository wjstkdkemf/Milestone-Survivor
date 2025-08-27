using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float Damage = 10;
    public float LifeTime=.25f;
    private void Start()
    {
        Destroy(gameObject,LifeTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            collision.GetComponent<IDamageable>().TakeDamage(Damage);
         
           
        }
    }
  
}
