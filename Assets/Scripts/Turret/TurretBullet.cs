using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    [SerializeField] private float speed = 5.0f;
    [HideInInspector] public Transform EnemyPosition;
    Vector3 direction;
    [SerializeField] private bool IsActived = true;
    private bool hasHit;
    private DoDamage damageComponent;

    private void OnEnable()
    {
        IsActived = true;
        hasHit = false;
        damageComponent = GetComponent<DoDamage>();

        StartCoroutine(Reset());
        Invoke ("CalculateDirection", .01f);    
    }
    private void CalculateDirection()
    {          
        if(EnemyPosition != null) 
        direction = (EnemyPosition.position - transform.position).normalized;        
    }
    void Update()
    {      
        if(IsActived)
            transform.position += (direction) * (speed*(1+PlayerStats.Instance.projectileSpeedBonus/100)) * Time.deltaTime;
    }
    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(3);

        if (IsActived)
        {
            ObjectPoolingManager.instance.ReturnObjectToPool(this.gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (damageComponent != null && damageComponent.TryApplyDamage(collision))
        {
            hasHit = true; 
            //HandleSelfDestruction();
        }
    }
}