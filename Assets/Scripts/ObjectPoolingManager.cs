using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectPoolingManager : MonoBehaviour
{
    public static ObjectPoolingManager instance;
    public static List<PooledObjectInfo> objectPools = new List<PooledObjectInfo>();
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
        if (ObjectToSpawn == null)
        {         
            return null;
        }

        // Ensure the pool exists
        PooledObjectInfo pool = objectPools.Find(p => p.name == ObjectToSpawn.name);

        if (pool == null)
        {
            // Create a new pool if none exists
            pool = new PooledObjectInfo { name = ObjectToSpawn.name };
            objectPools.Add(pool);
        }

        // Clean up any null entries in the pool
        pool.gameObjects.RemoveAll(go => go == null);

        // Try to find an inactive object in the pool
        GameObject spawnableObject = pool.gameObjects.FirstOrDefault();

        if (spawnableObject == null)
        {
            // Instantiate a new object if no reusable objects are available
            spawnableObject = Instantiate(ObjectToSpawn, Position, Rotation);
            spawnableObject.transform.SetParent(ObjectPooledParent.transform);
        }
        else
        {
            // Reuse an existing object
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

        // Extract the original prefab name (excluding "(Clone)")
        string goName = Obj.name.Replace("(Clone)", "").Trim();

        PooledObjectInfo pool = objectPools.Find(p => p.name == goName);

        if (pool == null)
        {
            Debug.LogWarning($"No pool found for object '{goName}'. Destroying the object.");
            Destroy(Obj);
        }
        else
        {
            // Deactivate and return to pool
            Obj.SetActive(false);
            Obj.transform.SetParent(ObjectPooledParent.transform);
            pool.gameObjects.Add(Obj);
        }
    }
}
public class PooledObjectInfo
{
    public string name;
    public List<GameObject> gameObjects = new List<GameObject>();
}
