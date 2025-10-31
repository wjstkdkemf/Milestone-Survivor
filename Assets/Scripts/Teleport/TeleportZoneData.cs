using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TeleportZoneData
{
    public string zoneName; // UI의 첫 번째 ScrollView에 표시될 이름 (예: "숲", "동굴")
    public List<TeleportData> pointsInZone; // 이 그룹에 속한 텔레포트 포인트 데이터 목록
}
