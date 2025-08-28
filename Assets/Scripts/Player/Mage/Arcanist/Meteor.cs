using UnityEngine;

public class Meteor : MonoBehaviour
{
    [Header("Impact")]
    public float impactRadius = 4f; 
    public int damage = 100;
    public float fallSpeed = 20f;

    [Header("Effects")]
    public GameObject explosionPrefab;

    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = new Vector3(transform.position.x, transform.position.y - 15f, transform.position.z);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    void Explode()
    {
        // 1. Instantiate explosion visual effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // 2. Create a new GameObject for the damage area
        GameObject damageArea = new GameObject("MeteorDamageArea");
        damageArea.transform.position = transform.position;

        // 3. Add and configure the CircleCollider2D for the explosion radius
        CircleCollider2D collider = damageArea.AddComponent<CircleCollider2D>();
        collider.radius = impactRadius;
        collider.isTrigger = true;

        // 4. Add and configure the DoDamage script
        DoDamage doDamage = damageArea.AddComponent<DoDamage>();
        doDamage.damage = this.damage; // Pass the meteor's damage value
        doDamage.damageEnemy = true;
        doDamage.damagePlayer = false;
        doDamage.selfDestroy = true;      // The damage area should destroy itself
        doDamage.lifeTime = 0.1f;         // It only needs to exist for a moment to apply damage
        doDamage.IsUsingObjetPooling = false;

        // 5. Destroy the meteor itself
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}