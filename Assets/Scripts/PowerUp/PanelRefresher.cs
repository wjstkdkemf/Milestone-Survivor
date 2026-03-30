using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelRefresher : MonoBehaviour
{
private void OnEnable()
    {
        // 패널이 켜질 때마다 매니저의 골드 갱신 함수를 강제 호출
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.UpdateGoldUI();
            
            // 필요하다면 버튼들의 상태도 같이 갱신 (선택 사항)
            // PowerUpManager.Instance.RefreshAllButtons(); 
        }
    }
}
