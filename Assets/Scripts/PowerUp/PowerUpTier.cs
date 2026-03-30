using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PowerUpTier
{
    public string tierName; // "1차 각성", "2차 각성"
    public List<PowerUpScriptableObject> tierPowerUps; // 해당 티어의 파워업 데이터들

    // 이 회차의 모든 파워업이 MAX인지 확인하는 함수 (잠금 해제 조건용)
    public bool IsAllMaxed()
    {
        foreach (var powerUp in tierPowerUps)
        {
            if (powerUp.CurrentLevel < powerUp.upgradeValues.Length)
                return false;
        }
        return true;
    }
}
