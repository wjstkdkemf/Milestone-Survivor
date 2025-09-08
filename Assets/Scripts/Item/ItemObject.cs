using UnityEngine;
using System.Collections.Generic;
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
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(itemData.itemName);
                }
                Destroy(gameObject);
            }
        }
    }
}
