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
        PickupNearbyCollectibles();
    }
    private void PickupNearbyCollectibles()
    {
        if (trans == null || !trans.gameObject.activeInHierarchy)
            return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            trans.position, xpPickupRange, CollectiblesLayer);

        foreach (Collider2D col in colliders)
        {
            if (col == null) continue;

            if (col.TryGetComponent<XPCrystal>(out var crystal))
                crystal.Collect(trans);

            if (col.TryGetComponent<GoldCoin>(out var coin))
                coin.Collect(trans);

            if (col.TryGetComponent<ItemObject>(out var item))
                item.Collect(trans);
        }
    }

    public bool CollectEverything()
    {
        var crystals = FindObjectsByType<XPCrystal>(FindObjectsSortMode.None);
        var coins = FindObjectsByType<GoldCoin>(FindObjectsSortMode.None);
        var items = FindObjectsByType<ItemObject>(FindObjectsSortMode.None);

        if (crystals.Length == 0 && coins.Length == 0 && items.Length == 0)
            return true;

        if (trans == null || !trans.gameObject.activeInHierarchy)
            return false;

        foreach (var crystal in crystals)
            if (crystal != null)
                crystal.Collect(trans);

        foreach (var coin in coins)
            if (coin != null)
                coin.Collect(trans);

        foreach (var item in items)
            if (item != null)
                item.Collect(trans);

        return false;
    }
    public void CollectAllXpCrystals()
    {
        XPCrystal[] allCrystals = FindObjectsOfType<XPCrystal>();
        foreach (XPCrystal crystal in allCrystals)
        {
            if (crystal != null)
            {
                crystal.Collect(trans);
            }
        }
    }

    public void CollectAllGoldCoin()
    {
        GoldCoin[] allGoldCoins = FindObjectsOfType<GoldCoin>();
        foreach (GoldCoin goldCoin in allGoldCoins)
        {
            if (goldCoin != null)
            {
                goldCoin.Collect(trans);
            }
        }
    }
    public void CollectAllItems()
    {
        ItemObject[] allItems = FindObjectsOfType<ItemObject>();
        foreach (ItemObject item in allItems)
        {
            if (item != null)
            {
                item.Collect(trans);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, xpPickupRange);
    }
}
