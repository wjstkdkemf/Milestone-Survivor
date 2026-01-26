using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectPoolingManager : MonoBehaviour
{
    public static ObjectPoolingManager instance;
    private Dictionary<string, PooledObjectInfo> poolDictionary = new Dictionary<string, PooledObjectInfo>();
    public List<PooledObjectInfo> objectPools = new List<PooledObjectInfo>(); // 인스펙터 확인용
    private static GameObject ObjectPooledParent;

    private void Awake()
    {
        // Ensure singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Create a parent object for pooled objects
        if (ObjectPooledParent == null)
        {
            ObjectPooledParent = new GameObject("ObjectPoolParent");
        }
        // 딕셔너리 초기화
        foreach (var pool in objectPools)
        {
            poolDictionary[pool.name] = pool;
        }
    }

    private void Update()
    {
        // Debugging pooled objects (optional)
#if UNITY_EDITOR
        // if (Input.GetMouseButtonDown(0))
        // {
        //     foreach (PooledObjectInfo obj in objectPools)
        //     {
        //         Debug.Log($"Pool: {obj.name}, Count: {obj.gameObjects.Count}");
        //     }
        // }
#endif
    }


    public GameObject spawnGameObject(GameObject ObjectToSpawn, Vector3 Position, Quaternion Rotation)
    {
        if (ObjectToSpawn == null || GameManager.Instance.Pause)
        {         
            return null;
        }

        string prefabName = ObjectToSpawn.name;
        PooledObjectInfo pool;

        if (!poolDictionary.TryGetValue(prefabName, out pool))
        {
            pool = new PooledObjectInfo { name = prefabName };
            objectPools.Add(pool);
            poolDictionary.Add(prefabName, pool);
        }

        // Clean up any null entries in the pool (안전장치)
        if (pool.gameObjects != null)
        {
            pool.gameObjects.RemoveAll(go => go == null);
        }

        // Try to find an inactive object in the pool
        GameObject spawnableObject = pool.gameObjects.FirstOrDefault();

        if (spawnableObject == null)
        {
            if (ObjectPooledParent == null || GameManager.Instance.Pause)
            {
                return null;
            }
            spawnableObject = Instantiate(ObjectToSpawn, Position, Rotation);
            spawnableObject.name = prefabName; 
            spawnableObject.transform.SetParent(ObjectPooledParent.transform);
        }
        else
        {
            spawnableObject.transform.position = Position;
            spawnableObject.transform.rotation = Rotation;
            pool.gameObjects.Remove(spawnableObject);
            spawnableObject.SetActive(true);
        }

        return spawnableObject;
    }
    public void ReturnObjectToPool(GameObject Obj)
    {
        if (Obj == null)
        {
            Debug.LogWarning("Attempted to return a null object to the pool.");
            return;
        }

        string goName = Obj.name;
        while (goName.EndsWith("(Clone)"))
        {
            goName = goName.Substring(0, goName.Length - 7);
        }
        goName = goName.Trim();

        if (poolDictionary.TryGetValue(goName, out PooledObjectInfo pool))
        {
            Obj.SetActive(false);
            if (ObjectPooledParent != null)
                Obj.transform.SetParent(ObjectPooledParent.transform);
                
            pool.gameObjects.Add(Obj);
        }
        else
        {
            Debug.LogWarning($"Pool not found for '{goName}'. Destroying.");
            Destroy(Obj);
        }
    }
}
public class PooledObjectInfo
{
    public string name;
    public List<GameObject> gameObjects = new List<GameObject>();
}
