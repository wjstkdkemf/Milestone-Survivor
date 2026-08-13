using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class TeleportZoneData
{
    public string zoneName;
    [SerializeField] private LocalizedString localizedZoneName;
    public Sprite zoneSpirte;
    public List<TeleportData> pointsInZone;
    public GameObject customMapPrefab;
    public bool useCustomMapPrefab;

    public string GetZoneName()
    {
        if (localizedZoneName != null && !localizedZoneName.IsEmpty)
        {
            string localized = localizedZoneName.GetLocalizedString();

            if (!string.IsNullOrEmpty(localized))
                return localized;
        }

        return zoneName;
    }
}
