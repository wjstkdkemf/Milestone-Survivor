using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorChanger : MonoBehaviour
{
[Header("설정: 연결될 층 번호")]
    [SerializeField] private int lowFloorIndex = 1;  // 아래층 (예: 1층)
    [SerializeField] private int highFloorIndex = 2; // 위층 (예: 2층)

    [Header("설정: 플레이어 물리 레이어 이름")]
    [SerializeField] private string playerLayerF1 = "Player_F1"; // 1층일 때 플레이어 레이어
    [SerializeField] private string playerLayerF2 = "Player_F2"; // 2층일 때 플레이어 레이어

    [Header("설정: 시각적 높이 (Sorting Order)")]
    // 2층으로 가면 플레이어가 1층 바닥보다 확실히 앞에 그려져야 함
    [SerializeField] private int orderInLayerLow = 10; 
    [SerializeField] private int orderInLayerHigh = 20;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어만 반응
        if (collision.CompareTag("Player"))
        {
            // 계단의 중심(transform.position.y)보다 
            // 플레이어가 아래에서 들어왔으면 -> 올라가는 중
            // 플레이어가 위에서 들어왔으면 -> 내려가는 중
            Teleporter playerScript = collision.GetComponent<Teleporter>();

            if (playerScript == null) return;
            
            if (collision.transform.position.y < transform.position.y)
            {
                GoUp(playerScript);
            }
            else
            {
                GoDown(playerScript);
            }
        }
    }

    // ▲ 2층으로 올라갈 때 실행
    private void GoUp(Teleporter player)
    {
        Debug.Log("2층으로 올라감!");
        // 1. 물리 레이어 변경 (2층 벽이랑만 부딪히게)
        player.SetFloorInfo(highFloorIndex, playerLayerF2, orderInLayerHigh);
    }

    // ▼ 1층으로 내려갈 때 실행
    private void GoDown(Teleporter player)
    {
        Debug.Log("1층으로 내려감!");

        player.SetFloorInfo(lowFloorIndex, playerLayerF1, orderInLayerLow);
    }
}
