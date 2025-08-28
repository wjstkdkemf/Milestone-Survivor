using System.Collections;
using UnityEngine;

public class GoldCoin : MonoBehaviour
{
    public int GoldValue = 1;
    public float attractionSpeed = 15f; // Speed can be adjusted for a better feel
    private bool isCollected = false;

    public void Collect(Transform playerTransform)
    {
        if (!isCollected)
        {
            isCollected = true;
            StartCoroutine(MoveAndCollect(playerTransform));
        }
    }

    private IEnumerator MoveAndCollect(Transform playerTransform)
    {
        // Disable the collider while moving
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }

        // Move towards the player
        while (transform != null && Vector3.Distance(transform.position, playerTransform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, attractionSpeed * Time.deltaTime);
            yield return null;
        }

        // Grant coin and destroy
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddCoin(GoldValue);
        }
        Destroy(gameObject);
    }
}
