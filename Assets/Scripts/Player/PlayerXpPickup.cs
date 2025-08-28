using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerXpPickup : MonoBehaviour
{

    public float xpPickupRange = 5f; // Default pickup range

    public LayerMask CollectiblesLayer; // LayerMask to filter XP Crystals and Coins

    private void Update()
    {
        // Automatically pick up nearby items
        PickupNearbyXpCrystals();
        PickupNearbyGoldCoin();

        if (GameManager.Instance.AllKill == true)
        {
            CollectAllXpCrystals();
            CollectAllGoldCoin();
            GameManager.Instance.AllKill = false;
        }
    }


    void PickupNearbyXpCrystals()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, xpPickupRange, CollectiblesLayer);

        foreach (Collider2D collider in hitColliders)
        {
            XPCrystal crystal = collider.GetComponent<XPCrystal>();
            if (crystal != null)
            {
                crystal.Collect(this);
            }
        }
    }

    void PickupNearbyGoldCoin()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, xpPickupRange, CollectiblesLayer);

        foreach (Collider2D collider in hitColliders)
        {
            GoldCoin goldCoin = collider.GetComponent<GoldCoin>();
            if (goldCoin != null)
            {
                goldCoin.Collect(transform); // Pass player transform to the coin
            }
        }
    }

    public void CollectAllXpCrystals()
    {
        StartCoroutine(CollectAllXpCrystalsRoutine());
    }

    private IEnumerator CollectAllXpCrystalsRoutine()
    {
        XPCrystal[] allCrystals = FindObjectsOfType<XPCrystal>();
        foreach (XPCrystal crystal in allCrystals)
        {
            if (crystal != null)
            {
                crystal.Collect(this);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    public void CollectAllGoldCoin()
    {
        StartCoroutine(CollectAllGoldCoinRoutine());
    }

    private IEnumerator CollectAllGoldCoinRoutine()
    {
        GoldCoin[] allGoldCoins = FindObjectsOfType<GoldCoin>();
        foreach (GoldCoin goldCoin in allGoldCoins)
        {
            if (goldCoin != null)
            {
                goldCoin.Collect(transform);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, xpPickupRange);
    }
}
