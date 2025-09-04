using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public ItemData itemData;
    public float moveSpeed = 8f;
    private Transform player;
    private bool isCollecting = false;

    public void Collect(Transform playerTransform)
    {
        if (!isCollecting)
        {
            player = playerTransform;
            isCollecting = true;
        }
    }

    private void Update()
    {
        if (isCollecting && player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, player.position) < 0.5f)
            {
                // Here you would typically add the item to the player's inventory
                // For now, we'll just log it.
                Debug.Log("Picked up: " + itemData.itemName);
                Destroy(gameObject);
            }
        }
    }
}
