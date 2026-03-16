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
    public bool destroyAfterHit = true;
    public bool IsUsingObjetPooling;
    public float lifeTime = 3f;
    [Header("Cooldown Settings")]
    [SerializeField] private float startWaitTime = 0.2f;
    private float waitTime;

    // Layer references
    [SerializeField] public LayerMask playerLayer;
    [SerializeField] public LayerMask enemyLayer;
    [SerializeField] public LayerMask wallLayer;
    [SerializeField] public LayerMask shieldLayer;

    private void OnEnable()
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

    public bool TryApplyDamage(Collider2D collision)
    {
        if (this.enabled == false || this.gameObject.activeInHierarchy == false) 
            return false;

        if (waitTime > 0) return false; // Skip if cooldown hasn't expired

        int collidedLayer = collision.gameObject.layer;
        bool hitSuccess = false;

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

        return hitSuccess;
    }
    public bool TryCheakEnemy(Collider2D collision)
    {
        if (this.enabled == false || this.gameObject.activeInHierarchy == false) 
            return false;

        int collidedLayer = collision.gameObject.layer;

        if (((1 << collidedLayer) & enemyLayer) != 0 && damageEnemy)
        {
            return true;
        }
        return false;
    }

    private void DamagePlayer(Collider2D collision)
    {
        IDamageable playerHealth = collision.GetComponent<IDamageable>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage*(1-(PlayerStats.Instance.ArmorBonus / (PlayerStats.Instance.ArmorBonus + PlayerStats.Instance.NormalArmorRatio))));
            if (destroyAfterHit)
            {
                HandleSelfDestruction();
            }
            ResetCooldown();
        }
    }

    private void DamageEnemy(Collider2D collision)
    {
        int id = collision.gameObject.GetInstanceID();
        IDamageable enemy = ObjectPoolingManager.instance.GetDamageable(id);
        float Deal = PlayerStats.Instance.DamageBonus * damage;
        if(Random.value <= PlayerStats.Instance.DoubleDamageChance)
            Deal *= 2;
        if (enemy != null)
        {
            enemy.TakeDamage(Deal);//damage는 실제 해당 스킬의 계수로 결정.
            if (destroyAfterHit)
            {
                HandleSelfDestruction();
            }
            ResetCooldown();
        }
    }

    private void HandleWallCollision()
    {
        HandleSelfDestruction();
        /*
        if (destroyAfterHit)
        {
            HandleSelfDestruction();
        }
        */
    }

    private void HandleSelfDestruction()
    {
        if (selfDestroy)
        {
            if (IsUsingObjetPooling)
                ObjectPoolingManager.instance.ReturnObjectToPool(this.gameObject);
            else 
                Destroy(gameObject);
        }
        else
        {
             // 보통은 그냥 Destroy합니다. (안전을 위해)
             if (!IsUsingObjetPooling) Destroy(gameObject);
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