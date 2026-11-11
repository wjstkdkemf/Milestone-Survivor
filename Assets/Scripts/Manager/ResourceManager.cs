using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    // Key: 에셋의 고유 키 (RuntimeKey), Value: 로딩 핸들
    private Dictionary<string, AsyncOperationHandle<GameObject>> loadedAssets = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    private readonly Dictionary<string, PendingLoad> pendingLoads = new();
    private bool isShuttingDown;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        isShuttingDown = true;
        StopAllCoroutines();

        var pending = new List<PendingLoad>(pendingLoads.Values);
        var loaded = new List<AsyncOperationHandle<GameObject>>(loadedAssets.Values);
        pendingLoads.Clear();
        loadedAssets.Clear();

        foreach (var request in pending)
        {
            if (request.Handle.IsValid())
                Addressables.Release(request.Handle);

            request.Handle = default;
        }

        foreach (var handle in loaded)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        foreach (var request in pending)
            request.Complete(null);

        if (Instance == this)
            Instance = null;
    }

    // 외부에서 에셋 로딩을 부탁할 때 쓰는 함수
    // 콜백(Action)을 사용하여 로딩이 완료되면 알려줍니다.
    public void LoadAsset(AssetReference reference, Action<GameObject> onLoaded)
    {
        string key = reference != null && reference.RuntimeKeyIsValid()
            ? reference.RuntimeKey.ToString()
            : null;

        LoadAssetByKey(key, onLoaded);
    }
    // 외부에서 에셋 사용이 끝났을 때 메모리 반환을 부탁하는 함수
    public void UnloadAsset(AssetReference reference)
    {
        string key = reference.RuntimeKey.ToString();

        if (loadedAssets.ContainsKey(key))
        {
            // 메모리 해제 및 장부에서 삭제
            Addressables.Release(loadedAssets[key]);
            loadedAssets.Remove(key);
            // Debug.Log($"[ResourceManager] 메모리 해제 완료: {key}");
        }
    }
    // 문자열(Address) 기반 로딩 함수
    public void LoadAssetByKey(string key, Action<GameObject> onLoaded)
    {
        var request = new PendingLoad();
        request.Callbacks.Add(onLoaded);

        if (isShuttingDown || string.IsNullOrEmpty(key) || !gameObject.activeInHierarchy)
        {
            request.Complete(null);
            return;
        }

        if (loadedAssets.TryGetValue(key, out var loadedHandle))
        {
            request.Complete(loadedHandle.Result);
            return;
        }

        if (pendingLoads.TryGetValue(key, out var pending))
        {
            pending.Callbacks.Add(onLoaded);
            return;
        }

        pendingLoads.Add(key, request);
        StartCoroutine(LoadAssetByKeyCoroutine(key, request));
    }

    private IEnumerator LoadAssetByKeyCoroutine(string key, PendingLoad request)
    {
        AsyncOperationHandle<GameObject> handle = default;
        try
        {
            handle = Addressables.LoadAssetAsync<GameObject>(key);
            request.Handle = handle;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }

        if (!handle.IsValid())
        {
            pendingLoads.Remove(key);
            request.Complete(null);
            yield break;
        }

        yield return handle;

        if (isShuttingDown || request.IsCompleted)
            yield break;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (!loadedAssets.ContainsKey(key))
            {
                loadedAssets.Add(key, handle);
                pendingLoads.Remove(key);
                request.Complete(handle.Result);
            }
            else
            {
                Addressables.Release(handle);
                AsyncOperationHandle<GameObject> existingHandle = loadedAssets[key];
                pendingLoads.Remove(key);
                request.Complete(existingHandle.Result);
            }
        }
        else
        {
            Debug.LogError($"[ResourceManager] 맵/에셋 로딩 실패: {key}");
            Addressables.Release(handle);
            pendingLoads.Remove(key);
            request.Complete(null);
        }
    }

    // 문자열(Address) 기반 해제 함수
    public void UnloadAssetByKey(string key)
    {
        if (loadedAssets.ContainsKey(key))
        {
            Addressables.Release(loadedAssets[key]);
            loadedAssets.Remove(key);
        }
    }

    private sealed class PendingLoad
    {
        public AsyncOperationHandle<GameObject> Handle;
        public readonly List<Action<GameObject>> Callbacks = new();

        public bool IsCompleted { get; private set; }

        public void Complete(GameObject result)
        {
            if (IsCompleted)
                return;

            IsCompleted = true;

            var callbacks = Callbacks.ToArray();
            Callbacks.Clear();

            foreach (var callback in callbacks)
            {
                try
                {
                    callback?.Invoke(result);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }
}