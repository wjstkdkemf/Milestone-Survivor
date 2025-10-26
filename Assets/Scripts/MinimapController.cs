using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour
{
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
}
