using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTilemapManager : MonoBehaviour
{
    public GameObject chunkPrefab;               // Prefab for each chunk
    public int chunkSize = 10;                   // Size of each chunk in world units
    public int loadRadius = 3;                   // Radius (in chunks) to load around the player

    public GameObject backgroundPrefab;          // Prefab for background (spawns once per chunk)
    public List<GameObject> resourcePrefabs;     // List of resource prefabs (e.g., trees, rocks)
    public int resourceCountPerChunk = 5;        // Number of resource objects per chunk

    private Transform player;
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Queue<GameObject> chunkPool = new Queue<GameObject>(); // Pool to reuse chunks
    private float chunkUpdateDelay = 0.5f;       // Delay between chunk updates
    private float lastChunkUpdateTime;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;  // Ensure the player has the "Player" tag
        InitializeChunkPool();
        UpdateChunks();
    }

    private void Update()
    {
        // Only check for chunk updates after a delay
        if (Time.time - lastChunkUpdateTime >= chunkUpdateDelay)
        {
            lastChunkUpdateTime = Time.time;
            UpdateChunks();
        }
    }

    private void InitializeChunkPool()
    {
        // Create a pool of chunks to avoid instantiation/destruction
        int initialPoolSize = (loadRadius * 2 + 1) * (loadRadius * 2 + 1);
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject pooledChunk = Instantiate(chunkPrefab);
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

        // Unload chunks that are too far from the player
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (Vector2Int coord in activeChunks.Keys)
        {
            if (!requiredChunks.Contains(coord))
            {
                chunksToUnload.Add(coord);
            }
        }

        foreach (Vector2Int coord in chunksToUnload)
        {
            UnloadChunk(coord);
        }
    }

    private void LoadChunk(Vector2Int coord)
    {
        GameObject chunk;

        // Use a chunk from the pool if available, otherwise instantiate a new one
        if (chunkPool.Count > 0)
        {
            chunk = chunkPool.Dequeue();
            chunk.transform.position = new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0);
            chunk.SetActive(true);
        }
        else
        {
            chunk = Instantiate(chunkPrefab, new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0), Quaternion.identity, transform);
        }

        // Populate the chunk with background and resource objects
        GenerateChunkContent(chunk, coord);
        activeChunks.Add(coord, chunk);
    }

    private void UnloadChunk(Vector2Int coord)
    {
        if (activeChunks.TryGetValue(coord, out GameObject chunk))
        {
            foreach (Transform child in chunk.transform)
            {
                Destroy(child.gameObject);  // Clear all children objects when unloading
            }

            chunk.SetActive(false);              // Deactivate instead of destroying
            activeChunks.Remove(coord);
            chunkPool.Enqueue(chunk);            // Return to the pool for reuse
        }
    }

    private void GenerateChunkContent(GameObject chunk, Vector2Int chunkCoord)
    {
        // Seeded random generation to ensure the same layout each time the chunk is loaded
        System.Random random = new System.Random(chunkCoord.GetHashCode());

        // Spawn the background object once per chunk
        GameObject background = Instantiate(backgroundPrefab, chunk.transform);
        background.transform.localPosition = Vector3.zero;

        // Spawn resource objects (trees, rocks) multiple times per chunk
        for (int i = 0; i < resourceCountPerChunk; i++)
        {
            GameObject resourcePrefab = resourcePrefabs[random.Next(resourcePrefabs.Count)];
            GameObject resource = Instantiate(resourcePrefab, chunk.transform);

            // Randomize position within the chunk
            float posX = random.Next(0, chunkSize);
            float posY = random.Next(0, chunkSize);
            resource.transform.localPosition = new Vector3(posX, posY, 0);
        }
    }
}