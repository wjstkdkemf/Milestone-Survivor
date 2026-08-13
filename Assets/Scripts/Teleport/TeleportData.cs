using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewTeleportData", menuName = "Teleport/Teleport Point Data")]
public class TeleportData : ScriptableObject
{
    [Header("Map Node ID")]
    public string nodeID;

    [Header("Target")]
    public string pointID;
    public string targetSpawnPointID;

    [Header("UI")]
    public string displayName;
    [SerializeField] private LocalizedString localizedDisplayName;
    [SerializeField] private LocalizedString localizedMapLabel;
    public Sprite mapIcon;

    [Header("Progress")]
    public bool isUnlocked;

    [Header("Map UI")]
    public Vector2 mapPosition;
    public string mapLabel;
    public List<TeleportData> connectedPoints;
    public TeleportNodeType nodeType;

    [Header("Preview")]
    public Sprite previewImage;
    public LocalizedString TeleportDes;

    public enum TeleportNodeType
    {
        Normal,
        Entrance,
        Center,
        Reward,
        Boss
    }

    public string GetNodeID()
    {
        if (!string.IsNullOrEmpty(nodeID))
            return nodeID;

        if (!string.IsNullOrEmpty(targetSpawnPointID))
            return targetSpawnPointID;

        if (!string.IsNullOrEmpty(displayName))
            return displayName;

        return name;
    }

    public string GetTargetMapID()
    {
        return pointID;
    }

    public string GetDisplayName()
    {
        string fallback = string.IsNullOrEmpty(displayName) ? mapLabel : displayName;
        return GetLocalizedString(localizedDisplayName, fallback);
    }

    public string GetMapLabel()
    {
        string fallback = string.IsNullOrEmpty(mapLabel) ? GetDisplayName() : mapLabel;
        return GetLocalizedString(localizedMapLabel, fallback);
    }

    public string GetDescription()
    {
        return GetLocalizedString(TeleportDes, "");
    }

    private static string GetLocalizedString(LocalizedString localizedString, string fallback)
    {
        if (localizedString != null && !localizedString.IsEmpty)
        {
            string localized = localizedString.GetLocalizedString();

            if (!string.IsNullOrEmpty(localized))
                return localized;
        }

        return fallback;
    }
}
