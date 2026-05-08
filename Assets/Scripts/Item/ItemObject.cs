using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using InventorySystem;
public class ItemObject : MonoBehaviour
{
    public ItemData itemData;
    public float moveSpeed = 8f;
    private bool isCollecting = false;

    private void OnEnable()
    {
        isCollecting = false; // 상태 초기화

        // "지금 스테이지 클리어 상태인가?" 확인
        if (GameManager.Instance != null && GameManager.Instance.AllKill)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Collect(playerObj.transform);
            }
        }
    }

    public void Collect(Transform playerTransform)
    {
        if (!isCollecting)
        {
            isCollecting = true;
            // 코루틴으로 이동 로직 위임 (Update보다 효율적)
            StopAllCoroutines();
            StartCoroutine(MoveAndCollect(playerTransform));
        }
    }

    // [핵심 2] XPCrystal과 동일한 Lerp 움직임 적용
    private IEnumerator MoveAndCollect(Transform playerTransform)
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        float minSpeed = 10f; 
        float distanceWeight = 5f; 

        while (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) > 0.1f)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            float currentSpeed = Mathf.Max(minSpeed, distance * distanceWeight);

            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);
            
            yield return null;
        }
        if (InventoryController.instance != null && itemData != null)
        {
            // "ClearInventory"라는 이름의 인벤토리로 아이템 1개 추가
            DropGold();
            InventoryController.instance.AddItem("ClearInventory", itemData.itemName, 1);
        }

        // 제거 혹은 비활성화
        Destroy(gameObject); 
        // 만약 풀링을 쓴다면: gameObject.SetActive(false);
    }
    private void DropGold()
    {
        PlayerStats.Instance.AddGold(itemData.price);
    }
}
