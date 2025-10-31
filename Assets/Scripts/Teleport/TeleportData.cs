using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTeleportData", menuName = "Teleport/Teleport Point Data")]
public class TeleportData : ScriptableObject
{
    [Header("고유 ID")]
    public string pointID; // 예: "Forest_Start", "Cave_BossRoom"

    [Header("UI 표시 정보")]
    public string displayName; // 예: "고요한 숲 입구"
    public Sprite mapIcon;     // 맵 UI에 표시할 아이콘
    
    [Header("맵 이동 정보")]
    //public string targetMapAddress; // 로드할 맵 프리팹의 주소 (어드레서블 권장)
    public string targetSpawnPointID; // 이 맵에 도착했을 때 스폰될 위치의 ID

    [Header("게임 진행 정보")]
    public bool isUnlocked; // 플레이어가 이 포인트를 활성화했는지
}
