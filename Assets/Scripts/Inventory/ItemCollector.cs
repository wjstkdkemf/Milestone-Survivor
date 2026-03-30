using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ItemCollector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        ItemObject itemObject = other.GetComponent<ItemObject>();
        if (itemObject != null)
        {
            // This will make the item move towards the player
            itemObject.Collect(transform);
        }
    }
}
