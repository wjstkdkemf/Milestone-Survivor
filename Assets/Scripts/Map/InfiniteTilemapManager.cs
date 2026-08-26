using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;
using Unity.VisualScripting;
using Unity.Collections;

[System.Serializable]
public class MapTheme
{
    public string themeName;
    public GameObject chunkPrefab;
    public GameObject backgroundPrefab;
    public GameObject borderWallPrefab;
    public List<GameObject> resourcePrefabs;
    public int resourceCountPerChunk = 5;
}

public class InfiniteTilemapManager : MonoBehaviour
{
    public static InfiniteTilemapManager Instance;

    [Header("Map Themes")]
    public List<MapTheme> mapThemes;

    [Header("Map Configuration")]
    public Vector3 battleMapStartPosition = new Vector3(1000, 1000, 0);
    public int chunkSize = 10;
    public int loadRadius = 3;
    [Header("PoolingResourse")]
    public int resourcePoolInitialSize = 20;

    private MapTheme currentTheme;
    private Transform player;
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private GameObject BorderWall;
    private Queue<GameObject> chunkPool = new Queue<GameObject>();
    private Dictionary<GameObject, Queue<GameObject>> resourcePools = new Dictionary<GameObject, Queue<GameObject>>();
    private float chunkUpdateDelay = 0.5f;
    private float lastChunkUpdateTime;
    private bool isMapActive = false;

    private HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();
    private List<Vector2Int> chunksToUnload = new List<Vector2Int>();
    private Vector2Int lastPlayerChunkCoord = Vector2Int.one * int.MaxValue;
    public MapChunkManager mapChunkManager;
    private int currentMapSeed;
    [Header("Job System Grid Data")]
    public NativeParallelHashMap<Vector2Int, byte> globalWallMap;
    private void Awake()
    {
        Instance = this;
        globalWallMap = new NativeParallelHashMap<Vector2Int, byte>(10000, Allocator.Persistent);
    }
    private void Start()
    {
        var playerGameObject = GameObject.FindWithTag("Player");
        if(playerGameObject != null)
            player = playerGameObject.transform;
        //GenerateMap(mapThemes[0].themeName);
    }
    private void OnDestroy()
    {
        Instance = null;
        if (globalWallMap.IsCreated) globalWallMap.Dispose();
    }

    private void Update()
    {
        if (!isMapActive || player == null) return;

        if (Time.time - lastChunkUpdateTime >= chunkUpdateDelay)
        {
            lastChunkUpdateTime = Time.time;

            Vector2Int playerChunkCoord = new Vector2Int(
                Mathf.FloorToInt(player.position.x / chunkSize),
                Mathf.FloorToInt(player.position.y / chunkSize)
            );

            if (playerChunkCoord != lastPlayerChunkCoord)
            {
                UpdateChunks(playerChunkCoord); // 좌표를 인자로 넘김
                mapChunkManager.OnMapChunkUpdated();
                lastPlayerChunkCoord = playerChunkCoord;
            }
        }
    }

    public void GenerateMap(string themeName)
    {
        currentTheme = mapThemes.FirstOrDefault(t => t.themeName == themeName);
        if (currentTheme == null)
        {
            Debug.LogError($"Map theme '{themeName}' not found.");
            return;
        } 

        currentMapSeed = UnityEngine.Random.Range(0, int.MaxValue);

        if (player != null)
        {
            player.transform.position = battleMapStartPosition;
        }
        
        transform.position = battleMapStartPosition;

        InitializeChunkPool();
        InitializeResourcePools();

        BorderWall = Instantiate(currentTheme.borderWallPrefab);
        BorderWall.transform.position = battleMapStartPosition;

        CinemachineVirtualCamera encounterCam = BorderWall.GetComponentInChildren<CinemachineVirtualCamera>();

        if (encounterCam != null)
        {
            encounterCam.Follow = player;
        }

        isMapActive = true;

        lastPlayerChunkCoord = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.y / chunkSize)
        );
        UpdateChunks(lastPlayerChunkCoord);
        mapChunkManager.OnMapChunkUpdated();
    }

    public void ClearMap()
    {
        isMapActive = false;
        globalWallMap.Clear();

        List<Vector2Int> allActiveCoords = activeChunks.Keys.ToList();
        foreach (Vector2Int coord in allActiveCoords)
        {
            UnloadChunk(coord);
        }
        activeChunks.Clear();

        foreach (var chunk in chunkPool)
        {
            if (chunk != null) Destroy(chunk);
        }
        chunkPool.Clear();

        //리소스 풀도 모두 파괴
        foreach (var pool in resourcePools.Values)
        {
            foreach (var resource in pool)
            {
                if(resource != null) Destroy(resource);
            }
        }
        resourcePools.Clear();
        requiredChunks.Clear();
        chunksToUnload.Clear();

        if(BorderWall != null) Destroy(BorderWall);

        BorderWall = null;
        
        lastPlayerChunkCoord = Vector2Int.one * int.MaxValue;
    }

    private void InitializeChunkPool()
    {
        if (currentTheme == null) return;

        int initialPoolSize = (loadRadius * 2 + 1) * (loadRadius * 2 + 1);
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject pooledChunk = Instantiate(currentTheme.chunkPrefab);
            pooledChunk.transform.SetParent(transform);
            pooledChunk.SetActive(false);
            chunkPool.Enqueue(pooledChunk);
        }
    }
    //리소스 풀을 초기화하는 함수
    private void InitializeResourcePools()
    {
        if (currentTheme == null) return;

        // 배경 프리팹 풀 생성
        if (currentTheme.backgroundPrefab != null)
        {
            CreatePoolForPrefab(currentTheme.backgroundPrefab, resourcePoolInitialSize);
        }
        
        // 리소스 프리팹 풀 생성
        if (currentTheme.resourcePrefabs != null)
        {
            foreach (var prefab in currentTheme.resourcePrefabs)
            {
                if (prefab != null)
                {
                    CreatePoolForPrefab(prefab, resourcePoolInitialSize);
                }
            }
        }
    }
    
    //특정 프리팹을 위한 풀 생성 헬퍼
    private void CreatePoolForPrefab(GameObject prefab, int size)
    {
        if (!resourcePools.ContainsKey(prefab))
        {
            resourcePools[prefab] = new Queue<GameObject>();
            for (int i = 0; i < size; i++)
            {
                GameObject instance = Instantiate(prefab);
                instance.transform.SetParent(transform); // Manager 하위에 둠
                instance.SetActive(false);
                
                // PooledResource 컴포넌트 추가 및 원본 프리팹 정보 저장
                PooledResource pr = instance.GetComponent<PooledResource>();
                if (pr == null) pr = instance.AddComponent<PooledResource>();
                pr.originalPrefab = prefab;
                
                resourcePools[prefab].Enqueue(instance);
            }
        }
    }

    //Update()에서 분리, GC Alloc 최적화
    private void UpdateChunks(Vector2Int playerChunkCoord)
    {
        // 재사용 컬렉션 초기화
        requiredChunks.Clear();
        chunksToUnload.Clear();
        
        // 필요한 청크 계산 및 로드
        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int y = -loadRadius; y <= loadRadius; y++)
            {
                Vector2Int chunkCoord = playerChunkCoord + new Vector2Int(x, y);
                requiredChunks.Add(chunkCoord); // 미리 만들어둔 HashSet 사용

                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    LoadChunk(chunkCoord);
                }
            }
        }

        foreach (Vector2Int coord in activeChunks.Keys)
        {
            if (!requiredChunks.Contains(coord))
            {
                chunksToUnload.Add(coord); // 미리 만들어둔 List 사용
            }
        }
        
        // 재사용 리스트를 기반으로 청크 언로드
        foreach (Vector2Int coord in chunksToUnload)
        {
            UnloadChunk(coord);
        }
    }

    private void LoadChunk(Vector2Int coord)
    {
        if (currentTheme == null) return;

        GameObject chunk;
        Vector3 position = new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0);

        if (chunkPool.Count > 0)
        {
            chunk = chunkPool.Dequeue();
        }
        else
        {
            chunk = Instantiate(currentTheme.chunkPrefab);
            chunk.transform.SetParent(transform);
        }

        chunk.transform.position = position;
        chunk.SetActive(true);

        GenerateChunkContent(chunk, coord); //내용물 생성 로직 호출
        activeChunks.Add(coord, chunk);

        TilemapGridChunk gridChunk = chunk.GetComponentInChildren<TilemapGridChunk>();
        if (gridChunk != null)
        {
            gridChunk.RegisterWallsToGlobalMap();
        }
    }

    private void UnloadChunk(Vector2Int coord)
    {
        if (activeChunks.TryGetValue(coord, out GameObject chunk))
        {
            TilemapGridChunk gridChunk = chunk.GetComponentInChildren<TilemapGridChunk>();
            if (gridChunk != null)
            {
                gridChunk.UnregisterWallsFromGlobalMap();
            }

            ReturnChunkContentToPool(chunk); 

            chunk.SetActive(false);
            activeChunks.Remove(coord);
            chunkPool.Enqueue(chunk);
        }
    }

    private void GenerateChunkContent(GameObject chunk, Vector2Int chunkCoord)
    {
        if (currentTheme == null) return;

        System.Random random = new System.Random(chunkCoord.GetHashCode() ^ currentMapSeed);

        if (currentTheme.backgroundPrefab != null)
        {
            GetResourceFromPool(currentTheme.backgroundPrefab, Vector3.zero, chunk.transform);
        }

        if (currentTheme.resourcePrefabs != null && currentTheme.resourcePrefabs.Count > 0)
        {
            for (int i = 0; i < currentTheme.resourceCountPerChunk; i++)
            {
                GameObject resourcePrefab = currentTheme.resourcePrefabs[random.Next(currentTheme.resourcePrefabs.Count)];

                //로컬 좌표 계산
                float posX = (float)(random.NextDouble() * chunkSize) - chunkSize * 0.5f; // 청크 중앙 기준
                float posY = (float)(random.NextDouble() * chunkSize) - chunkSize * 0.5f;
                Vector3 localPos = new Vector3(posX, posY, 0);

                GetResourceFromPool(resourcePrefab, localPos, chunk.transform);
            }
        }
    }
    private GameObject GetResourceFromPool(GameObject prefab, Vector3 localPosition, Transform parent)
    {
        Queue<GameObject> pool;
        
        // 해당 프리팹의 풀이 있는지 확인, 없으면 새로 생성
        if (!resourcePools.TryGetValue(prefab, out pool))
        {
            pool = new Queue<GameObject>();
            resourcePools[prefab] = pool;
        }

        GameObject instance;
        if (pool.Count > 0)
        {
            instance = pool.Dequeue(); // 풀에서 가져오기
        }
        else
        {
            // 풀이 비었으면 새로 생성 (예외 처리)
            instance = Instantiate(prefab);
            PooledResource pr = instance.AddComponent<PooledResource>();
            pr.originalPrefab = prefab;
        }

        instance.transform.SetParent(parent);
        instance.transform.localPosition = localPosition;
        instance.SetActive(true);
        return instance;
    }
    
    private void ReturnChunkContentToPool(GameObject chunk)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in chunk.transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            PooledResource pr = child.GetComponent<PooledResource>();
            if (pr != null && pr.originalPrefab != null)
            {
                // 이 리소스의 원본 프리팹에 해당하는 풀을 찾음
                if (resourcePools.TryGetValue(pr.originalPrefab, out Queue<GameObject> pool))
                {
                    child.gameObject.SetActive(false);
                    child.SetParent(transform); // 다시 Manager 하위로 이동
                    pool.Enqueue(child.gameObject); // 풀에 반납
                }
                else
                {
                    // 풀이 없는 비정상적인 경우
                    Destroy(child.gameObject);
                }
            }
            else
            {
                // 풀링 대상이 아닌 자식 오브젝트 (예: 타일맵 자체)는 파괴하지 않음
                // 만약 타일맵 외에 풀링 안되는 자식이 있다면 Destroy(child.gameObject);
            }
        }
    }
}