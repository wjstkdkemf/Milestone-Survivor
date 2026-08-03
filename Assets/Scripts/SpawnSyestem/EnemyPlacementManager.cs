using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPlacementManager : MonoBehaviour
{
    public static EnemyPlacementManager Instance { get; private set; }

    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private int maxAttempts = 10;
    [SerializeField] private float navMeshSampleDistance = 2f;
    [SerializeField, Min(0f)] private float outsideWorldPadding = 1.5f;
    [SerializeField, Min(0f)] private float recycleViewportPadding = 0.45f;
    [SerializeField, Min(1)] private int maxRepositionsPerFrame = 20;
    [SerializeField] private bool is2DGame = true;

    private readonly Queue<RepositionRequest> requests = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "EnemyPlacementManager가 두 개 이상 존재합니다. " +
                "먼저 활성화된 인스턴스를 사용합니다.",
                this);
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Configure(
        Camera camera,
        LayerMask walls,
        int attempts,
        bool use2DPhysics)
    {
        targetCamera = camera != null ? camera : Camera.main;
        wallLayerMask = walls;
        maxAttempts = Mathf.Max(1, attempts);
        is2DGame = use2DPhysics;
    }

    public void RequestReposition(
        Enemy enemy,
        RepositionReason reason,
        int requestVersion)
    {
        if (enemy == null)
        {
            return;
        }

        requests.Enqueue(
            new RepositionRequest(enemy, reason, requestVersion));
    }

    private void Update()
    {
        int processCount =
            Mathf.Min(maxRepositionsPerFrame, requests.Count);

        for (int i = 0; i < processCount; i++)
        {
            RepositionRequest request = requests.Dequeue();

            if (request.Enemy == null ||
                !request.Enemy.isActiveAndEnabled ||
                !request.Enemy.IsRepositionRequestCurrent(request.Version))
            {
                continue;
            }

            if (TryFindOutsideCameraPosition(
                    request.Enemy.CollisionRadius,
                    request.Enemy.RequiresNavMesh,
                    false,
                    out Vector3 position))
            {
                request.Enemy.ApplyReposition(position);
            }
            else
            {
                request.Enemy.FinishReposition(false);
            }
        }
    }

    public bool TryFindOutsideCameraPosition(
        float collisionRadius,
        bool requiresNavMesh,
        bool onlySideSpawn,
        out Vector3 position)
    {
        position = Vector3.zero;

        Camera camera = ResolveCamera();
        if (camera == null)
        {
            return false;
        }

        float radius = Mathf.Max(0.05f, collisionRadius);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 candidate = GetRandomOutsideCameraPosition(
                camera,
                onlySideSpawn,
                radius);

            if (TryResolveOutsidePosition(
                    camera,
                    candidate,
                    radius,
                    requiresNavMesh,
                    out position))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryFindOutsideCameraPositionAtAngle(
        float angleDegrees,
        float collisionRadius,
        bool requiresNavMesh,
        out Vector3 position)
    {
        position = Vector3.zero;

        Camera camera = ResolveCamera();
        if (camera == null)
        {
            return false;
        }

        float radius = Mathf.Max(0.05f, collisionRadius);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float maxAngleOffset =
                Mathf.Min(30f, attempt * 5f);
            float angleOffset = attempt == 0
                ? 0f
                : Random.Range(
                    -maxAngleOffset,
                    maxAngleOffset);

            float angleRadians =
                (angleDegrees + angleOffset) * Mathf.Deg2Rad;

            Vector2 direction = new(
                Mathf.Cos(angleRadians),
                Mathf.Sin(angleRadians));

            Vector3 candidate =
                GetOutsideCameraPositionAtDirection(
                    camera,
                    direction,
                    radius);

            if (TryResolveOutsidePosition(
                    camera,
                    candidate,
                    radius,
                    requiresNavMesh,
                    out position))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryFindOutsideCameraPositionNear(
        Vector3 center,
        float spreadRadius,
        float collisionRadius,
        bool requiresNavMesh,
        out Vector3 position)
    {
        position = Vector3.zero;

        Camera camera = ResolveCamera();
        if (camera == null)
        {
            return false;
        }

        float radius = Mathf.Max(0.05f, collisionRadius);
        float spread = Mathf.Max(0f, spreadRadius);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 offset =
                Random.insideUnitCircle * spread;

            Vector3 candidate = center +
                new Vector3(offset.x, offset.y, 0f);

            if (TryResolveOutsidePosition(
                    camera,
                    candidate,
                    radius,
                    requiresNavMesh,
                    out position))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsOutsideCamera(Vector3 worldPosition)
    {
        Camera camera = ResolveCamera();
        return camera != null && IsOutsideCamera(camera, worldPosition);
    }

    public bool IsBeyondRecycleBounds(Vector3 worldPosition)
    {
        Camera camera = ResolveCamera();
        if (camera == null)
        {
            return true;
        }

        Vector3 viewportPosition =
            camera.WorldToViewportPoint(worldPosition);

        return viewportPosition.z <= 0f ||
               viewportPosition.x < -recycleViewportPadding ||
               viewportPosition.x > 1f + recycleViewportPadding ||
               viewportPosition.y < -recycleViewportPadding ||
               viewportPosition.y > 1f + recycleViewportPadding;
    }

    private Camera ResolveCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        return targetCamera;
    }

    private Vector3 GetRandomOutsideCameraPosition(
        Camera camera,
        bool onlySideSpawn,
        float radius)
    {
        int side = onlySideSpawn
            ? Random.Range(2, 4)
            : Random.Range(0, 4);

        GetCameraWorldBounds(
            camera,
            out float minX,
            out float maxX,
            out float minY,
            out float maxY);

        float outsideOffset =
            radius + outsideWorldPadding;

        Vector3 result;

        switch (side)
        {
            case 0:
                result = new Vector3(
                    Random.Range(minX, maxX),
                    maxY + outsideOffset,
                    0f);
                break;
            case 1:
                result = new Vector3(
                    Random.Range(minX, maxX),
                    minY - outsideOffset,
                    0f);
                break;
            case 2:
                result = new Vector3(
                    minX - outsideOffset,
                    Random.Range(minY, maxY),
                    0f);
                break;
            default:
                result = new Vector3(
                    maxX + outsideOffset,
                    Random.Range(minY, maxY),
                    0f);
                break;
        }

        return result;
    }

    private Vector3 GetOutsideCameraPositionAtDirection(
        Camera camera,
        Vector2 direction,
        float radius)
    {
        GetCameraWorldBounds(
            camera,
            out float minX,
            out float maxX,
            out float minY,
            out float maxY);

        Vector2 center = new(
            (minX + maxX) * 0.5f,
            (minY + maxY) * 0.5f);

        float halfWidth = (maxX - minX) * 0.5f;
        float halfHeight = (maxY - minY) * 0.5f;

        float xDistance = Mathf.Abs(direction.x) > 0.0001f
            ? halfWidth / Mathf.Abs(direction.x)
            : float.PositiveInfinity;

        float yDistance = Mathf.Abs(direction.y) > 0.0001f
            ? halfHeight / Mathf.Abs(direction.y)
            : float.PositiveInfinity;

        float distanceToEdge =
            Mathf.Min(xDistance, yDistance);
        float edgeDirectionComponent =
            xDistance <= yDistance
                ? Mathf.Abs(direction.x)
                : Mathf.Abs(direction.y);
        float distanceBeyondEdge =
            (radius + outsideWorldPadding) /
            Mathf.Max(0.0001f, edgeDirectionComponent);

        Vector2 result = center +
            direction *
            (distanceToEdge + distanceBeyondEdge);

        return new Vector3(result.x, result.y, 0f);
    }

    private bool TryResolveOutsidePosition(
        Camera camera,
        Vector3 candidate,
        float radius,
        bool requiresNavMesh,
        out Vector3 position)
    {
        position = Vector3.zero;
        candidate.z = 0f;

        if (requiresNavMesh)
        {
            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    navMeshSampleDistance,
                    NavMesh.AllAreas))
            {
                return false;
            }

            candidate = hit.position;
            candidate.z = 0f;
        }

        if (!IsFullyOutsideCamera(
                camera,
                candidate,
                radius) ||
            OverlapsWall(candidate, radius))
        {
            return false;
        }

        position = candidate;
        return true;
    }

    private static void GetCameraWorldBounds(
        Camera camera,
        out float minX,
        out float maxX,
        out float minY,
        out float maxY)
    {
        float distanceToGameplayPlane =
            Mathf.Abs(camera.transform.position.z);

        Vector3 bottomLeft = camera.ViewportToWorldPoint(
            new Vector3(
                0f,
                0f,
                distanceToGameplayPlane));

        Vector3 topRight = camera.ViewportToWorldPoint(
            new Vector3(
                1f,
                1f,
                distanceToGameplayPlane));

        minX = Mathf.Min(bottomLeft.x, topRight.x);
        maxX = Mathf.Max(bottomLeft.x, topRight.x);
        minY = Mathf.Min(bottomLeft.y, topRight.y);
        maxY = Mathf.Max(bottomLeft.y, topRight.y);
    }

    private static bool IsFullyOutsideCamera(
        Camera camera,
        Vector3 worldPosition,
        float radius)
    {
        GetCameraWorldBounds(
            camera,
            out float minX,
            out float maxX,
            out float minY,
            out float maxY);

        return worldPosition.x + radius <= minX ||
               worldPosition.x - radius >= maxX ||
               worldPosition.y + radius <= minY ||
               worldPosition.y - radius >= maxY;
    }

    private static bool IsOutsideCamera(
        Camera camera,
        Vector3 worldPosition)
    {
        Vector3 viewportPosition =
            camera.WorldToViewportPoint(worldPosition);

        return viewportPosition.z > 0f &&
               (viewportPosition.x < 0f ||
                viewportPosition.x > 1f ||
                viewportPosition.y < 0f ||
                viewportPosition.y > 1f);
    }

    private bool OverlapsWall(
        Vector3 position,
        float radius)
    {
        if (is2DGame)
        {
            return Physics2D.OverlapCircle(
                position,
                radius,
                wallLayerMask) != null;
        }

        return Physics.CheckSphere(
            position,
            radius,
            wallLayerMask);
    }

    private readonly struct RepositionRequest
    {
        public readonly Enemy Enemy;
        public readonly RepositionReason Reason;
        public readonly int Version;

        public RepositionRequest(
            Enemy enemy,
            RepositionReason reason,
            int version)
        {
            Enemy = enemy;
            Reason = reason;
            Version = version;
        }
    }
}
