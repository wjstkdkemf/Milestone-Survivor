using UnityEngine;
using System.Collections.Generic;
using InventorySystem;
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
                if (InventoryController.instance != null)
                {
                    InventoryController.instance.AddItem("ClearInventory", itemData.itemName, 1);//아이템 드랍 처리 부분.
                }
                Destroy(gameObject);
            }
        }
    }
}
