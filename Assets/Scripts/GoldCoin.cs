using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldCoin : MonoBehaviour
{
    public int GoldValue = 1;
    private bool isCollected = false;
    public float attractionSpeed = 5f; // Speed at which crystals move toward the player

    private Transform targetPlayer;

    void Update()
    {
        if (targetPlayer != null && !isCollected)
        {
            // Move the crystal toward the player when within pickup range
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, attractionSpeed * Time.deltaTime);
        }
    }

    public void AttractToPlayer(Transform player)
    {
        targetPlayer = player;
    }

    public void Collect()
    {
        if (!isCollected)
        {
          PlayerStats.Instance.AddCoin(GoldValue);
            isCollected = true;
            Destroy(gameObject);
        }
    }
}