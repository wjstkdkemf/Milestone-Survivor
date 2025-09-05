using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

[System.Serializable]
public class MapTheme
{
    public string themeName;
    public GameObject chunkPrefab;
    public GameObject backgroundPrefab;
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

    private MapTheme currentTheme;
    private Transform player;
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Queue<GameObject> chunkPool = new Queue<GameObject>();
    private float chunkUpdateDelay = 0.5f;
    private float lastChunkUpdateTime;
    private bool isMapActive = false;

    private void Start()
    {
        Instance = this;
        player = GameObject.FindWithTag("Player").transform;

        //GenerateMap(mapThemes[0].themeName);
    }

    private void Update()
    {
        if (!isMapActive || player == null) return;

        if (Time.time - lastChunkUpdateTime >= chunkUpdateDelay)
        {
            lastChunkUpdateTime = Time.time;
            UpdateChunks();
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

        

        if (player != null)
        {
            player.transform.position = battleMapStartPosition;
        }
        
        transform.position = battleMapStartPosition;

        InitializeChunkPool();
        isMapActive = true;
        UpdateChunks();
    }

    public void ClearMap()
    {
        isMapActive = false;
        foreach (var chunk in activeChunks.Values)
        {
            if(chunk != null) Destroy(chunk);
        }
        activeChunks.Clear();

        foreach (var chunk in chunkPool)
        {
            if(chunk != null) Destroy(chunk);
        }
        chunkPool.Clear();
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

    private void UpdateChunks()
    {
        Vector2Int playerChunkCoord = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.y / chunkSize)
        );

        HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();
        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int y = -loadRadius; y <= loadRadius; y++)
            {
                Vector2Int chunkCoord = playerChunkCoord + new Vector2Int(x, y);
                requiredChunks.Add(chunkCoord);

                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    LoadChunk(chunkCoord);
                }
            }
        }

        List<Vector2Int> chunksToUnload = activeChunks.Keys.Where(coord => !requiredChunks.Contains(coord)).ToList();
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
            chunk.transform.position = position;
            chunk.SetActive(true);
        }
        else
        {
            chunk = Instantiate(currentTheme.chunkPrefab, position, Quaternion.identity, transform);
        }

        GenerateChunkContent(chunk, coord);
        activeChunks.Add(coord, chunk);
    }

    private void UnloadChunk(Vector2Int coord)
    {
        if (activeChunks.TryGetValue(coord, out GameObject chunk))
        {
            // Clear children before returning to pool
            foreach (Transform child in chunk.transform)
            {
                Destroy(child.gameObject);
            }

            chunk.SetActive(false);
            activeChunks.Remove(coord);
            chunkPool.Enqueue(chunk);
        }
    }

    private void GenerateChunkContent(GameObject chunk, Vector2Int chunkCoord)
    {
        if (currentTheme == null) return;

        System.Random random = new System.Random(chunkCoord.GetHashCode());

        if (currentTheme.backgroundPrefab != null)
        {
            GameObject background = Instantiate(currentTheme.backgroundPrefab, chunk.transform);
            background.transform.localPosition = Vector3.zero;
        }

        if (currentTheme.resourcePrefabs != null && currentTheme.resourcePrefabs.Count > 0)
        {
            for (int i = 0; i < currentTheme.resourceCountPerChunk; i++)
            {
                GameObject resourcePrefab = currentTheme.resourcePrefabs[random.Next(currentTheme.resourcePrefabs.Count)];
                GameObject resource = Instantiate(resourcePrefab, chunk.transform);

                float posX = (float)(random.NextDouble() * chunkSize);
                float posY = (float)(random.NextDouble() * chunkSize);
                resource.transform.localPosition = new Vector3(posX, posY, 0);
            }
        }
    }
}