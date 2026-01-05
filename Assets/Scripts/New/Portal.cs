using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{

    [Tooltip("이 포탈을 탔을 때 로드할 맵 프리팹")]
    public string defaultMapName;

    [Tooltip("새 맵에서 플레이어가 생성될 위치")]
    public string defaultSpawnPoint;

    [Header("조건부 이동 위치 (확장 가능)")]
    [Tooltip("특정 업적을 달성했을 때 우선적으로 이동할 맵 목록")]
    public List<ConditionalMapLink> conditionalLinks;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !other.isTrigger)
        {
            return; // 플레이어가 아니면 아무것도 하지 않음
        }

        if (other.GetComponent<Teleporter>().isTeleporting) return;

        other.GetComponent<Teleporter>().isTeleporting = true;

        for (int i = conditionalLinks.Count - 1; i >= 0; i--)//역순체크 -> 추후 추가되는 업적이 더 상위 업적일것으로 예상.
        {
            ConditionalMapLink link = conditionalLinks[i];

            // 3. GameProgressManager를 통해 업적 달성 여부 확인
            //    GameProgressManager.Instance가 싱글톤이라고 가정합니다.
            if (GameProgressManager.Instance.IsUnlocked(link.requiredProgressID))
            {
                // 4. 조건 만족! 이 맵으로 즉시 이동하고 함수 종료
                Debug.Log($"조건 '{link.requiredProgressID}' 만족. '{link.targetMapName}'(으)로 이동합니다.");
                MainMapManager.Instance.ChangeMap(link.targetMapName, link.targetSpawnPoint);
                return; // 이동했으므로 더 이상 검사할 필요 없음
            }
        }

        MainMapManager.Instance.ChangeMap(defaultMapName, defaultSpawnPoint);
    }
}

[System.Serializable]
public class ConditionalMapLink
{
    [Tooltip("이 연결을 활성화하는 데 필요한 진행도 ID (업적 ID)")]
    public string requiredProgressID; // "Map" 변수를 대체합니다.
    [Tooltip("로드할 맵의 이름")]
    public string targetMapName;      // "targetMapPrefab" 리스트를 대체합니다.
    [Tooltip("해당 맵에서 스폰될 위치 이름")]
    public string targetSpawnPoint; // "targetSpawnPosition"을 대체합니다.
}
