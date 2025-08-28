using System.Collections;
using UnityEngine;

public class XPCrystal : MonoBehaviour
{
    public int xpValue = 10;
    public float attractionSpeed = 15f; // Speed can be adjusted for a better feel
    private bool isCollected = false;

    public void Collect(PlayerXpPickup player)
    {
        if (!isCollected)
        {
            isCollected = true;
            StartCoroutine(MoveAndCollect(player.transform));
        }
    }

    private IEnumerator MoveAndCollect(Transform playerTransform)
    {
        // Disable the collider while moving to prevent it from being triggered again by other mechanics
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }

        // Move towards the player until very close
        while (transform != null && Vector3.Distance(transform.position, playerTransform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, attractionSpeed * Time.deltaTime);
            yield return null;
        }

        // Final processing: grant XP and destroy the object
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddXP(xpValue);
        }
        Destroy(gameObject);
    }
}
