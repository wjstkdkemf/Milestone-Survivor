using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set;}
    public Transform player;
    public Camera minimapCamera;
    public RectTransform minimapImage; // Changed from RawImage
    public GameObject playerMarker;
    public GameObject enemyMarkerPrefab;
    [Range(0f,10f)]
    public float Yaxiscorrection = 4.0f;

    private Dictionary<Transform, GameObject> enemyMarkers = new Dictionary<Transform, GameObject>();

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        // Find all initial enemies and create markers
        InvokeRepeating("FindAndCreateEnemyMarkers", 0f, 1f); // Periodically check for new enemies
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // Center the minimap camera on the player
            Vector3 newPosition = player.position;
            newPosition.y = minimapCamera.transform.position.y;
            minimapCamera.transform.position = newPosition;

            // Rotate the player marker to match the player's rotation
            playerMarker.transform.rotation = Quaternion.Euler(0f, 0f, -player.eulerAngles.y);
        }

        // Update enemy markers
        UpdateEnemyMarkers();
    }

    void FindAndCreateEnemyMarkers()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (!enemyMarkers.ContainsKey(enemy.transform))
            {
                AddEnemyMarker(enemy.transform);
            }
        }
    }

    void AddEnemyMarker(Transform enemy)
    {
        GameObject marker = Instantiate(enemyMarkerPrefab, minimapImage);
        enemyMarkers.Add(enemy, marker);
    }

    void UpdateEnemyMarkers()
    {
        List<Transform> enemiesToRemove = new List<Transform>();
        foreach (var enemyEntry in enemyMarkers)
        {
            Transform enemy = enemyEntry.Key;
            GameObject marker = enemyEntry.Value;

            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                Destroy(marker);
                enemiesToRemove.Add(enemy);
                continue;
            }

            Vector3 worldPos = enemy.position;
            Vector3 screenPos = minimapCamera.WorldToViewportPoint(worldPos);

            // Check if the enemy is within the camera's view
            if (screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1)
            {
                marker.SetActive(true);
                RectTransform markerRect = marker.GetComponent<RectTransform>();

                float yDifference = 0f;
                if (player != null)
                {
                    yDifference = enemy.position.y - player.position.y;
                }
                // Adjust the multiplier to control how much height difference affects the position
                float yPixelOffset = yDifference * Yaxiscorrection; //Y축 보정정도

                markerRect.anchoredPosition = new Vector2(
                    (screenPos.x * minimapImage.rect.width) - (minimapImage.rect.width / 2),
                    ((screenPos.y * minimapImage.rect.height) - (minimapImage.rect.height / 2)) + yPixelOffset
                );
            }
            else
            {
                marker.SetActive(false);
            }
        }

        foreach (var enemy in enemiesToRemove)
        {
            enemyMarkers.Remove(enemy);
        }
    }
    public void ChangeFloor(int floorIndex)
    {
        // 1. [중요] Common 레이어는 기본으로 깔고 갑니다.
        // 몬스터 아이콘이 여기 포함되므로 항상 보이게 됩니다.
        int layerMask = 1 << LayerMask.NameToLayer("MiniMap_Common");

        // 2. 그 위에 현재 층의 배경(F1, F2...)만 추가합니다.
        string targetLayerName = "MiniMap_F" + floorIndex;
        layerMask |= 1 << LayerMask.NameToLayer(targetLayerName);

        minimapCamera.cullingMask = layerMask;
    }
}
