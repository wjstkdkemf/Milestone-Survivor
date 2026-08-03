using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class PowerUpTier
{
    public string tierName; // "1차 각성", "2차 각성"
    [SerializeField] private LocalizedString localizedTierName;
    public List<PowerUpScriptableObject> tierPowerUps; // 해당 티어의 파워업 데이터들

    public string GetLocalizedTierName()
    {
        if (localizedTierName != null && !localizedTierName.IsEmpty)
        {
            string localized = localizedTierName.GetLocalizedString();

            if (!string.IsNullOrEmpty(localized))
                return localized;
        }

        return tierName ?? string.Empty;
    }

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
