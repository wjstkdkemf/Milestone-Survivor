using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneDamageArea : MonoBehaviour
{
    public DoDamage damageComponent;
    
    // 장판 안에 있는 적들을 관리하는 리스트
    private List<Collider2D> targetsInside = new List<Collider2D>();
    
    [Header("Tick Settings")]
    public float tickRate = 0.5f; // 데미지가 들어가는 주기
    private float tickTimer;

    private void Awake()
    {
        damageComponent = GetComponent<DoDamage>();
    }

    private void OnEnable()
    {
        targetsInside.Clear();
        tickTimer = tickRate; // 켜지자마자 첫 타격을 주기 위해 조절 가능
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!targetsInside.Contains(collision))
        {
            targetsInside.Add(collision);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (targetsInside.Contains(collision))
        {
            targetsInside.Remove(collision);
        }
    }
    protected virtual void Update()
    {
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            ApplyAreaDamage();
            tickTimer = tickRate;
        }
    }

    private void ApplyAreaDamage()
    {
        if (damageComponent == null) return;

        for (int i = targetsInside.Count - 1; i >= 0; i--)
        {
            if (i >= targetsInside.Count) 
            {
                continue; 
            }

            Collider2D target = targetsInside[i];

            if (target == null || !target.gameObject.activeInHierarchy)
            {
                targetsInside.RemoveAt(i);
                continue;
            }

            damageComponent.TryApplyDamage(target);
        }
    }
}
