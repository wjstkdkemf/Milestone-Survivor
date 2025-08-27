using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LTGUI;

public class IceOrb : MonoBehaviour
{
    private float chanceDoubleDamage;
    private float baseDamage = 5;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetInfo(float _baseDamage, float _chanceDoubleDamage)
    {
        baseDamage= _baseDamage;
        chanceDoubleDamage= _chanceDoubleDamage;
    
    
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            bool isDoubleDamage = Random.Range(0f, 100f) < chanceDoubleDamage;
            float currentDamage = (baseDamage)  ;
            if (damageable != null)
            {
                damageable.TakeDamage(currentDamage);
            }

        }      
    }
}
