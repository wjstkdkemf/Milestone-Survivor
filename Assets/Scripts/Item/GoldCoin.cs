using System.Collections;
using UnityEngine;

public class GoldCoin : MonoBehaviour
{
    public int GoldValue = 1;
    public float attractionSpeed = 15f; // Speed can be adjusted for a better feel
    private bool isCollected = false;
    private Collider2D myCollider;
    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }
    private void OnEnable()
    {
        // 변수 초기화 (오브젝트 풀링 대비)
        isCollected = false;
        if (myCollider != null) myCollider.enabled = true;

        // 태어났는데 이미 상황 종료(AllKill) 상태라면? -> 즉시 플레이어에게 날아감
        if (GameManager.Instance != null && GameManager.Instance.AllKill)
        {
            // 플레이어를 찾아서 수거 명령 실행
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // PlayerXpPickup 컴포넌트가 필요하다면 가져오고, 아니면 Transform만 넘겨도 됨
                // 여기서는 기존 구조 유지를 위해 컴포넌트를 찾아서 넘김
                var pickup = player.GetComponent<Transform>();
                if (pickup != null)
                {
                    Collect(pickup);
                }
            }
        }
    }

    public void Collect(Transform playerTransform)
    {
        if (!isCollected)
        {
            isCollected = true;
            StopAllCoroutines();
            StartCoroutine(MoveAndCollect(playerTransform));
        }
    }

    private IEnumerator MoveAndCollect(Transform playerTransform)
    {
        // Disable the collider while moving
        if (myCollider != null)
        {
            myCollider.enabled = false;
        }

        float minSpeed = 10f; 
        float distanceWeight = 5f; 

        while (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) > 0.1f)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            float currentSpeed = Mathf.Max(minSpeed, distance * distanceWeight);

            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);
            
            yield return null;
        }

        // Grant coin and destroy
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddCoin(GoldValue);
        }
        Destroy(gameObject);
    }
}
