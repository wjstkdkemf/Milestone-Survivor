using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LTGUI;

public class DoDamage : MonoBehaviour
{

    [Header("Damage Settings")]
    public float damage = 5f;
    public bool damagePlayer = true;
    public bool damageEnemy = true;

    [Header("Self-Destruction Settings")]
    public bool selfDestroy = false;
    public bool IsUsingObjetPooling;
    public float lifeTime = 3f;
    [Header("Cooldown Settings")]
    [SerializeField] private float startWaitTime = 0.2f;
    private float waitTime;

    // Layer references
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask shieldLayer;

    private void Start()
    {
        waitTime = 0f; // Start with 0 to allow initial damage application
        if (selfDestroy)
            StartCoroutine(SelfDestroy(lifeTime));
    }

    private void Update()
    {
        if (waitTime > 0)
            waitTime -= Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (waitTime > 0) return; // Skip if cooldown hasn't expired

        int collidedLayer = collision.gameObject.layer;

        if (((1 << collidedLayer) & playerLayer) != 0 && damagePlayer)
        {
            DamagePlayer(collision);
        }
        else if (((1 << collidedLayer) & enemyLayer) != 0 && damageEnemy)
        {
            DamageEnemy(collision);
        }
        else if (((1 << collidedLayer) & wallLayer) != 0)
        {
            HandleWallCollision();
        }
    }

    private void DamagePlayer(Collider2D collision)
    {
        IDamageable playerHealth = collision.GetComponent<IDamageable>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage*(1-(PlayerStats.Instance.ArmorBonus/100)));
            HandleSelfDestruction();
            ResetCooldown();
        }
    }

    private void DamageEnemy(Collider2D collision)
    {
        IDamageable enemy = collision.GetComponent<IDamageable>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage*(1+PlayerStats.Instance.DamageBonus/100));
            HandleSelfDestruction();
            ResetCooldown();
        }
    }

    private void HandleWallCollision()
    {
        HandleSelfDestruction();
    }

    private void HandleSelfDestruction()
    {
        if (selfDestroy)
        {
            if(IsUsingObjetPooling)
            ObjectPoolingManager.instance.ReturnObjectToPool(this.gameObject);
            Destroy(gameObject);
        }
    }

    private void ResetCooldown()
    {
        waitTime = startWaitTime;
    }
    IEnumerator SelfDestroy(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        HandleSelfDestruction();
    }
}