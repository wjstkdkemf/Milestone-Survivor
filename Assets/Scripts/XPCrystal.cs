using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPCrystal : MonoBehaviour
{
    public int xpValue = 10;
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

    public void Collect(PlayerXpPickup player)
    {
        if (!isCollected)
        {
            PlayerStats.Instance.AddXP(xpValue);
            isCollected = true;
            Destroy(gameObject);
        }
    }
}