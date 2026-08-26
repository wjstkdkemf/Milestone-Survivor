using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TeleportMapMaker : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private RectTransform mapRoot;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private RectTransform customMapContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject linePrefab;

    [Header("Node Colors")]
    [SerializeField] private bool UseColor = false;
    [SerializeField] private Color unlockedColor = new Color(0.20f, 0.24f, 0.21f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.16f, 0.12f, 0.12f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.18f, 0.55f, 0.34f, 1f);
    [SerializeField] private Color bossColor = new Color(0.45f, 0.12f, 0.14f, 1f);
    [SerializeField] private Color rewardColor = new Color(0.54f, 0.39f, 0.15f, 1f);
    [Header("Node Image")]
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite bossSprite;
    [SerializeField] private Sprite rewardSprite;
    [Header("Line Colors")]
    [SerializeField] private Color lineColor = new Color(0.34f, 0.25f, 0.18f, 1f);
    [SerializeField] private Color lockedLineColor = new Color(0.18f, 0.14f, 0.12f, 1f);
    [SerializeField] private float lineWidth = 6f;

    [Header("Fallback Node Size")]
    [SerializeField] private Vector2 normalNodeSize = new Vector2(72f, 44f);
    [SerializeField] private Vector2 specialNodeSize = new Vector2(96f, 52f);

    private readonly Dictionary<string, TeleportData> nodeLookup = new Dictionary<string, TeleportData>();
    private readonly Dictionary<TeleportData, Button> nodeButtons = new Dictionary<TeleportData, Button>();
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private GameObject customMapInstance;
    private TeleportData selectedPoint;
    private Action<TeleportData, Button> onNodeSelected;

    [SerializeField] private TeleportPreviewMaker teleportPreviewMaker;

    private void Awake()
    {
        EnsureContainers();
    }

    public void DrawMap(TeleportZoneData zoneData, Action<TeleportData, Button> selectCallback)
    {
        EnsureContainers();
        ClearMap();
        ClearPreview();
        onNodeSelected = selectCallback;

        if (zoneData == null)
            return;

        if (zoneData.useCustomMapPrefab && zoneData.customMapPrefab != null)
        {
            RectTransform targetContainer = customMapContainer != null ? customMapContainer : mapRoot;
            customMapInstance = Instantiate(zoneData.customMapPrefab, targetContainer);
            return;
        }

        BuildPointLookup(zoneData);
        DrawLines(zoneData);
        DrawNodes(zoneData);
    }

    private void EnsureContainers()
    {
        if (mapRoot == null)
            mapRoot = GetComponent<RectTransform>();

        if (mapRoot == null)
            return;

        if (lineContainer == null)
            lineContainer = CreateContainer("LineContainer");

        if (nodeContainer == null)
            nodeContainer = CreateContainer("NodeContainer");

        if (customMapContainer == null)
            customMapContainer = CreateContainer("CustomMapContainer");

        lineContainer.SetAsFirstSibling();
        nodeContainer.SetAsLastSibling();
    }

    private RectTransform CreateContainer(string containerName)
    {
        Transform existing = mapRoot.Find(containerName);
        if (existing != null && existing.TryGetComponent(out RectTransform existingRect))
            return existingRect;

        GameObject container = new GameObject(containerName, typeof(RectTransform));
        container.transform.SetParent(mapRoot, false);

        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return rect;
    }

    public void SelectNode(TeleportData point)
    {
        selectedPoint = point;
        RefreshNodeColors();
    }

    public void ClearMap()
    {
        if (customMapInstance != null)
        {
            Destroy(customMapInstance);
            customMapInstance = null;
        }

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
                Destroy(spawnedObjects[i]);
        }

        spawnedObjects.Clear();
        nodeLookup.Clear();
        nodeButtons.Clear();
        selectedPoint = null;
    }

    private void BuildPointLookup(TeleportZoneData zoneData)
    {
        if (zoneData.pointsInZone == null)
            return;

        foreach (TeleportData point in zoneData.pointsInZone)
        {
            if (point == null)
                continue;

            string nodeID = point.GetNodeID();
            if (string.IsNullOrEmpty(nodeID))
                continue;

            if (nodeLookup.ContainsKey(nodeID))
            {
                Debug.LogWarning($"[TeleportMapMaker] Duplicate nodeID '{nodeID}' in zone '{zoneData.zoneName}'. Check TeleportData nodeID values.");
                continue;
            }

            nodeLookup.Add(nodeID, point);
        }
    }

    private void DrawLines(TeleportZoneData zoneData)
    {
        if (zoneData.pointsInZone == null)
            return;

        HashSet<string> drawnConnections = new HashSet<string>();

        foreach (TeleportData fromPoint in zoneData.pointsInZone)
        {
            if (fromPoint == null)
                continue;

            if (fromPoint.connectedPoints == null)
                continue;

            foreach (TeleportData connectedPoint in fromPoint.connectedPoints)
            {
                if (connectedPoint == null) continue;

                string connectedPointID = connectedPoint.GetNodeID();

                if (!nodeLookup.TryGetValue(connectedPointID, out TeleportData toPoint) || toPoint == null)
                    continue;

                string connectionKey = GetConnectionKey(fromPoint.GetNodeID(), toPoint.GetNodeID());
                if (!drawnConnections.Add(connectionKey))
                    continue;

                CreateLine(fromPoint, toPoint);
            }
        }
    }

    private void DrawNodes(TeleportZoneData zoneData)
    {
        if (zoneData.pointsInZone == null)
            return;

        foreach (TeleportData point in zoneData.pointsInZone)
        {
            if (point == null)
                continue;

            Button nodeButton = CreateNode(point);
            if (nodeButton != null)
                nodeButtons[point] = nodeButton;
        }

        RefreshNodeColors();
    }

    private Button CreateNode(TeleportData point)
    {
        RectTransform parent = nodeContainer != null ? nodeContainer : mapRoot;
        if (parent == null)
            return null;

        GameObject nodeObject = nodePrefab != null
            ? Instantiate(nodePrefab, parent)
            : CreateFallbackNode(parent);

        spawnedObjects.Add(nodeObject);
        nodeObject.name = $"MapNode_{point.GetNodeID()}";

        RectTransform nodeRect = nodeObject.GetComponent<RectTransform>();
        nodeRect.anchorMin = new Vector2(0.5f, 0.5f);
        nodeRect.anchorMax = new Vector2(0.5f, 0.5f);
        nodeRect.anchoredPosition = point.mapPosition;

        if (nodePrefab == null)
            nodeRect.sizeDelta = IsSpecialNode(point) ? specialNodeSize : normalNodeSize;

        TextMeshProUGUI label = nodeObject.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = point.GetMapLabel();

        Button button = nodeObject.GetComponent<Button>();
        if (button == null)
            button = nodeObject.AddComponent<Button>();

        bool unlocked = IsUnlocked(point);
        button.interactable = unlocked;

        if (unlocked)
        {
            TeleportData currentPoint = point;
            Button currentButton = button;
            button.onClick.AddListener(() =>
            {
                onNodeSelected?.Invoke(currentPoint, currentButton);
                ShowPreview(currentPoint);
                SelectNode(currentPoint);
            });
        }

        return button;
    }

    private void ShowPreview(TeleportData point)
    {
        if (teleportPreviewMaker != null)
            teleportPreviewMaker.Draw(point);
    }

    private void ClearPreview()
    {
        if (teleportPreviewMaker != null)
            teleportPreviewMaker.Clear();
    }

    private GameObject CreateFallbackNode(RectTransform parent)
    {
        GameObject nodeObject = new GameObject("MapNode", typeof(RectTransform), typeof(Image), typeof(Button));
        nodeObject.transform.SetParent(parent, false);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(nodeObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 22f;
        label.color = Color.white;

        return nodeObject;
    }

    private void CreateLine(TeleportData fromPoint, TeleportData toPoint)
    {
        RectTransform parent = lineContainer != null ? lineContainer : mapRoot;
        if (parent == null)
            return;

        GameObject lineObject = linePrefab != null
            ? Instantiate(linePrefab, parent)
            : new GameObject("MapLine", typeof(RectTransform), typeof(Image));

        lineObject.transform.SetParent(parent, false);
        spawnedObjects.Add(lineObject);
        lineObject.name = $"MapLine_{fromPoint.GetNodeID()}_{toPoint.GetNodeID()}";

        RectTransform lineRect = lineObject.GetComponent<RectTransform>();
        Image lineImage = lineObject.GetComponent<Image>();

        Vector2 start = fromPoint.mapPosition;
        Vector2 end = toPoint.mapPosition;
        Vector2 delta = end - start;

        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = start + delta * 0.5f;
        lineRect.sizeDelta = new Vector2(delta.magnitude, lineWidth);
        lineRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

        if (lineImage != null)
            lineImage.color = IsUnlocked(fromPoint) && IsUnlocked(toPoint) ? lineColor : lockedLineColor;
    }

    private void RefreshNodeColors()
    {
        foreach (var pair in nodeButtons)
        {
            TeleportData point = pair.Key;
            Button button = pair.Value;
            if (button == null)
                continue;

            Image image = button.GetComponent<Image>();
            if (image == null)
                continue;

            if(UseColor)
            {
                image.color = GetNodeColor(point);
            }
            else
            {
                image.color = Color.white;
                image.sprite = GetNodeSprite(point) == null ? image.sprite : GetNodeSprite(point);
            }
        }
    }

    private Color GetNodeColor(TeleportData point)
    {
        if (point == selectedPoint)
            return selectedColor;

        if (!IsUnlocked(point))
            return lockedColor;

        switch (point.nodeType)
        {
            case TeleportData.TeleportNodeType.Boss:
                return bossColor;
            case TeleportData.TeleportNodeType.Reward:
                return rewardColor;
            default:
                return unlockedColor;
        }
    }
    private Sprite GetNodeSprite(TeleportData point)
    {
        if (point == selectedPoint)
            return selectedSprite;

        if (!IsUnlocked(point))
            return lockedSprite;

        switch (point.nodeType)
        {
            case TeleportData.TeleportNodeType.Boss:
                return bossSprite;
            case TeleportData.TeleportNodeType.Reward:
                return rewardSprite;
            default:
                return unlockedSprite;
        }
    }

    private bool IsUnlocked(TeleportData point)
    {
        return TeleportManager.Instance == null ||
               TeleportManager.Instance.IsPointUnlocked(point.targetSpawnPointID);
    }

    private bool IsSpecialNode(TeleportData point)
    {
        return point.nodeType == TeleportData.TeleportNodeType.Center ||
               point.nodeType == TeleportData.TeleportNodeType.Boss ||
               point.nodeType == TeleportData.TeleportNodeType.Reward;
    }

    private string GetConnectionKey(string firstID, string secondID)
    {
        return string.CompareOrdinal(firstID, secondID) < 0
            ? $"{firstID}|{secondID}"
            : $"{secondID}|{firstID}";
    }
}
