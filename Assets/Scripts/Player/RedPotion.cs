using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedPotion : MonoBehaviour
{
    public float HealAmount = 10;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
        //      AudioManager.instance.PlaySound("Heal");
            collision.GetComponent<PlayerHealth>().Heal(HealAmount);
            Destroy(gameObject);
        }
    }
}

