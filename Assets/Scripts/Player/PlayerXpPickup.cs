using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerXpPickup : MonoBehaviour
{

    public float xpPickupRange = 5f; // Default pickup range

    public LayerMask CollectiblesLayer; // LayerMask to filter XP Crystals

    private void Update()
    {

        // Automatically pick up nearby XP crystals
        PickupNearbyXpCrystals();
        PickupNearbyGoldCoin();
        // Optional: Press key to pick up all crystals in the scene
        /* if (Input.GetKeyDown(KeyCode.C))
         {
             CollectAllXpCrystals(); // Pick up all crystals in the scene
        CollectAllGoldCoin();
         }*/

        if (GameManager.Instance.AllKill == true)
        {
            CollectAllXpCrystals();
            CollectAllGoldCoin();
            GameManager.Instance.AllKill = false;
        }
    }


    void PickupNearbyXpCrystals()
    {
        // Use Physics2D.OverlapCircleAll with a LayerMask for better performance
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, xpPickupRange, CollectiblesLayer);

        foreach (Collider2D collider in hitColliders)
        {
            XPCrystal crystal = collider.GetComponent<XPCrystal>();
            if (crystal != null)
            {
                crystal.Collect(this); // Collect the crystal if it's within range
            }
        }
    }
    void PickupNearbyGoldCoin()
    {
        // Use Physics2D.OverlapCircleAll with a LayerMask for better performance
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, xpPickupRange, CollectiblesLayer);

        foreach (Collider2D collider in hitColliders)
        {
            GoldCoin crystal = collider.GetComponent<GoldCoin>();
            if (crystal != null)
            {
                crystal.Collect(); // Collect the crystal if it's within range
            }
        }
    }

    // Method to collect all XP crystals in the scene
    public void CollectAllXpCrystals()
    {
        // Find all XP crystals in the scene by using the LayerMask
        XPCrystal[] allCrystals = FindObjectsOfType<XPCrystal>();

        foreach (XPCrystal crystal in allCrystals)
        {
            crystal.Collect(this); // Collect every crystal in the scene
        }
    }
    // Method to collect all XP crystals in the scene
    public void CollectAllGoldCoin()
    {
        // Find all XP crystals in the scene by using the LayerMask
        GoldCoin[] allCrystals = FindObjectsOfType<GoldCoin>();

        foreach (GoldCoin crystal in allCrystals)
        {
            PlayerStats.Instance.AddCoin(5);
            crystal.Collect(); // Collect every crystal in the scene

        }
    }

    // Optional: Draw the pickup range in the scene view for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, xpPickupRange);
    }
}