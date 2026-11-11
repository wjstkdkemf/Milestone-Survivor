using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectPoolingManager : MonoBehaviour
{
    public static ObjectPoolingManager Instance;
    private Dictionary<string, PooledObjectInfo> poolDictionary = new Dictionary<string, PooledObjectInfo>();
    public List<PooledObjectInfo> objectPools = new List<PooledObjectInfo>(); // 인스펙터 확인용
    private Transform creationRoot;
    private static GameObject ObjectPooledParent;
    private bool isClearingAllPools;
    private bool isReturningAllPools;
    private bool IsPoolCreationBlocked =>isClearingAllPools || isReturningAllPools;

    public void ClearAllPools() => ClearPools(null);

    public void ClearThemePools() => ClearPools(PoolScope.Theme);

    public Dictionary<int, IDamageable> damageableCache = new Dictionary<int, IDamageable>();

    public GameObject DamageTextOrigin;

    private void Awake()
    {
        // Ensure singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
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
    public IDamageable GetDamageable(int instanceID)
    {
        if (damageableCache.TryGetValue(instanceID, out IDamageable damageable))
        {
            return damageable;
        }
        return null;
    }
    public void PrewarmDamageText(int amount)
    {
        if (IsPoolCreationBlocked || DamageTextOrigin == null) return;

        PooledObjectInfo pool = GetOrCreatePool(DamageTextOrigin, PoolScope.Run);
        if (pool == null) return;

        for (int i = 0; i < amount; i++)
        {
            PoolMember member = CreatePoolMember(DamageTextOrigin, pool);
            if (member == null)
                break;

            pool.gameObjects.Add(member.gameObject);
        }
    }
    public GameObject spawnGameObject(GameObject ObjectToSpawn, Vector3 Position, Quaternion Rotation, PoolScope? requestedScope = null)
    {
        if (IsPoolCreationBlocked || ObjectToSpawn == null ||
            GameManager.Instance == null || GameManager.Instance.Pause)
            return null;

        PooledObjectInfo pool = GetOrCreatePool(ObjectToSpawn, requestedScope);

        if (pool == null) return null;

        if (pool.IsClearing) return null;
        if (pool.Prefab != null && pool.Prefab != ObjectToSpawn)
        {
            Debug.LogError($"Different prefabs share pool name: {pool.name}");
            return null;
        }

        pool.gameObjects.RemoveAll(go => go == null);

        GameObject obj = pool.gameObjects.FirstOrDefault();
        PoolMember member = obj != null
            ? obj.GetComponent<PoolMember>()
            : CreatePoolMember(ObjectToSpawn, pool);
        if (member == null || member.Owner != this ||
            member.Pool != pool || !member.TryMarkRented())
            return null;

        obj = member.gameObject;
        pool.gameObjects.Remove(obj);
        obj.transform.SetPositionAndRotation(Position, Rotation);
        obj.SetActive(true);

        return obj != null && member.IsRented && obj.activeInHierarchy ? obj : null;
    }
    private PoolMember CreatePoolMember(GameObject prefab, PooledObjectInfo pool)
    {
        if (IsPoolCreationBlocked || prefab == null || pool == null ||
            pool.IsClearing || ObjectPooledParent == null)
            return null;

        if (pool.Prefab != null && pool.Prefab != prefab)
        {
            Debug.LogError($"Different prefabs share pool name: {pool.name}");
            return null;
        }

        if (creationRoot == null)
        {
            GameObject root = new GameObject("PoolCreationRoot");
            root.SetActive(false);
            root.transform.SetParent(transform, false);
            creationRoot = root.transform;
        }

        GameObject obj = Instantiate(prefab, creationRoot);
        obj.SetActive(false);
        obj.name = pool.name;

        if (!obj.TryGetComponent<PoolMember>(out var member))
            member = obj.AddComponent<PoolMember>();

        pool.Prefab = prefab;
        member.Bind(this, pool);
        pool.AllMembers.Add(member);

        if (obj.TryGetComponent<IDamageable>(out var damageable))
            damageableCache[obj.GetInstanceID()] = damageable;

        obj.transform.SetParent(ObjectPooledParent.transform, false);
        return member;
    }
    public void ReturnObjectToPool(GameObject obj)
    {
        if (obj == null) return;
        if (!obj.TryGetComponent<PoolMember>(out var member) ||
            member.Owner == null || member.Pool == null)
        {
            damageableCache.Remove(obj.GetInstanceID());
            obj.SetActive(false);
            Destroy(obj);
            return;
        }
        if (member.Owner != this)
        {
            member.Owner.ReturnObjectToPool(obj);
            return;
        }
        if (!member.TryMarkReturned()) return;

        PooledObjectInfo pool = member.Pool;
        obj.SetActive(false);
        if (pool.IsClearing || ObjectPooledParent == null ||
            !poolDictionary.TryGetValue(pool.name, out var registeredPool) ||
            !ReferenceEquals(pool, registeredPool))
        {
            pool.AllMembers.Remove(member);
            damageableCache.Remove(obj.GetInstanceID());
            Destroy(obj);
            return;
        }
        obj.transform.SetParent(ObjectPooledParent.transform, true);
        pool.gameObjects.Add(obj);
    }
    public void ReturnAllActiveObjectsToPool()
    {
        if (IsPoolCreationBlocked) return;
        isReturningAllPools = true;

        try
        {
            PoolMember[] members = objectPools
                .Where(pool => pool != null && !pool.IsClearing)
                .SelectMany(pool => pool.AllMembers)
                .ToArray();

            foreach (var member in members)
            {
                if (member == null || member.Owner != this ||
                    member.Pool == null || member.Pool.IsClearing ||
                    !member.IsRented)
                    continue;

                ReturnObjectToPool(member.gameObject);
            }
        }
        finally
        {
            isReturningAllPools = false;
        }
    }
    private void ClearPools(PoolScope? scope)
    {
        if (IsPoolCreationBlocked)
        {
            Debug.LogWarning("Pool cleanup requested during another pool operation.");
            return;
        }
        isClearingAllPools = true;
        try
        {
            PooledObjectInfo[] targets = objectPools
                .Where(pool => pool != null &&
                    (!scope.HasValue || pool.Scope == scope.Value))
                .ToArray();

            foreach (var pool in targets)
                pool.IsClearing = true;

            foreach (var pool in targets)
            {
                foreach (var member in pool.AllMembers.ToArray())
                {
                    if (member == null) continue;

                    GameObject obj = member.gameObject;
                    member.TryMarkReturned();
                    damageableCache.Remove(obj.GetInstanceID());
                    obj.SetActive(false);
                    Destroy(obj);
                }
                pool.AllMembers.Clear();
                pool.gameObjects.Clear();
                pool.Prefab = null;

                if (poolDictionary.TryGetValue(pool.name, out var registered) &&
                    ReferenceEquals(pool, registered))
                    poolDictionary.Remove(pool.name);

                objectPools.Remove(pool);
            }

            if (!scope.HasValue)
                damageableCache.Clear();
        }
        finally
        {
            isClearingAllPools = false;
        }
    }
    public PooledObjectInfo GetOrCreatePool(GameObject prefab, PoolScope? requestedScope = null)
    {
        if (IsPoolCreationBlocked || prefab == null) return null;
        if (poolDictionary.TryGetValue(prefab.name, out var pool))
        {
            if (pool.IsClearing) return null;
            if ((pool.Prefab != null && pool.Prefab != prefab) ||
                (requestedScope.HasValue && pool.Scope != requestedScope.Value))
            {
                Debug.LogError($"Conflicting pool registration: {prefab.name}");
                return null;
            }
            pool.Prefab = prefab;
            return pool;
        }
        pool = new PooledObjectInfo
        {
            name = prefab.name,
            Prefab = prefab,
            Scope = requestedScope ?? PoolScope.Run
        };
        poolDictionary.Add(pool.name, pool);
        objectPools.Add(pool);
        return pool;
    }
}
public class PooledObjectInfo
{
    public string name;
    public GameObject Prefab;
    public bool IsClearing;
    public PoolScope Scope = PoolScope.Run;

    public List<GameObject> gameObjects = new List<GameObject>();
    public HashSet<PoolMember> AllMembers = new HashSet<PoolMember>();
}
public enum PoolScope
{
    Run,   // 던전 플레이 동안 유지하는 공통 풀
    Theme  // 현재 테마를 떠날 때 정리하는 풀
}
