using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerXpPickup : MonoBehaviour
{

    public float xpPickupRange = 5f; // Default pickup range
    public Transform trans;

    public LayerMask CollectiblesLayer; // LayerMask to filter XP Crystals and Coins

    private void Update()
    {
        // Automatically pick up nearby items
        PickupNearbyXpCrystals();
        PickupNearbyGoldCoin();
        PickupNearbyItems();
    }
    public void CollectEverything()
    {
        CollectAllXpCrystals();
        CollectAllGoldCoin();
        CollectAllItems();
    }

    void PickupNearbyXpCrystals()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(trans.position, xpPickupRange, CollectiblesLayer);

        foreach (Collider2D collider in hitColliders)
        {
            XPCrystal crystal = collider.GetComponent<XPCrystal>();
            if (crystal != null)
            {
                crystal.Collect(trans);
            }
        }
    }

    void PickupNearbyGoldCoin()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(trans.position, xpPickupRange, CollectiblesLayer);

        foreach (Collider2D collider in hitColliders)
        {
            GoldCoin goldCoin = collider.GetComponent<GoldCoin>();
            if (goldCoin != null)
            {
                goldCoin.Collect(trans); // Pass player transform to the coin
            }
        }
    }

    void PickupNearbyItems()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(trans.position, xpPickupRange, CollectiblesLayer);

        foreach (Collider2D collider in hitColliders)
        {
            if(!collider.TryGetComponent<ItemObject>(out var itemObject))
                continue;
            if(!itemObject.TryGetComponent<ItemDropEffect>(out var effect))
                continue;
            
            if (itemObject != null && effect.GetHasLand())
            {
                itemObject.Collect(trans);
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
                crystal.Collect(trans);
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
                goldCoin.Collect(trans);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    public void CollectAllItems()
    {
        StartCoroutine(CollectAllItemsRoutine());
    }

    private IEnumerator CollectAllItemsRoutine()
    {
        ItemObject[] allItems = FindObjectsOfType<ItemObject>();
        foreach (ItemObject item in allItems)
        {
            if (item != null)
            {
                item.Collect(trans);
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
