using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPlayer : MonoBehaviour
{
    public float HealAmount=10;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {

            collision.GetComponent<PlayerHealth>().Heal(HealAmount);
            Destroy(gameObject);
        }
    }
}
