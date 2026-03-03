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
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // 외부에서 에셋 로딩을 부탁할 때 쓰는 함수
    // 콜백(Action)을 사용하여 로딩이 완료되면 알려줍니다.
    public void LoadAsset(AssetReference reference, Action<GameObject> onLoaded)
    {
        string key = reference.RuntimeKey.ToString();

        // 이미 로딩 장부에 있다면? 즉시 콜백 실행
        if (loadedAssets.ContainsKey(key))
        {
            onLoaded?.Invoke(loadedAssets[key].Result);
            return;
        }

        // 장부에 없다면 진짜 비동기 로딩 시작
        StartCoroutine(LoadAssetCoroutine(reference, key, onLoaded));
    }

    private IEnumerator LoadAssetCoroutine(AssetReference reference, string key, Action<GameObject> onLoaded)
    {
        AsyncOperationHandle<GameObject> handle = reference.LoadAssetAsync<GameObject>();
        
        yield return handle; // 로딩 끝날 때까지 대기

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // 로딩 성공 시 장부에 기록하고 결과물을 넘겨줌
            loadedAssets.Add(key, handle);
            onLoaded?.Invoke(handle.Result);
        }
        else
        {
            Debug.LogError($"[ResourceManager] 로딩 실패: {key}");
            Addressables.Release(handle);
            onLoaded?.Invoke(null);
        }
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
        if (loadedAssets.ContainsKey(key))
        {
            onLoaded?.Invoke(loadedAssets[key].Result);
            return;
        }
        StartCoroutine(LoadAssetByKeyCoroutine(key, onLoaded));
    }

    private IEnumerator LoadAssetByKeyCoroutine(string key, Action<GameObject> onLoaded)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(key);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedAssets.Add(key, handle);
            onLoaded?.Invoke(handle.Result);
        }
        else
        {
            Debug.LogError($"[ResourceManager] 맵/에셋 로딩 실패: {key}");
            Addressables.Release(handle);
            onLoaded?.Invoke(null);
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
}