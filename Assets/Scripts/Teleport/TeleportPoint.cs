
using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    public string teleportPointName;
    public string teleportMapName;
    [Header("이곳의 층 정보")]
    public int floorIndex = 1; // 1층인지 2층인지
    
    [Header("적용할 플레이어 설정")]
    public string targetLayerName = "Player_F1"; // 도착 시 변경될 물리 레이어
    public int targetSortingOrder = 10;          // 도착 시 변경될 그리기 순서


    void OnEnable()
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.RegisterPoint(this);
        }
    }

    void OnDisable()
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.UnRegisterPoint(this);
        }
    }

}
