using System.Collections;
using System.Collections.Generic;
//using System.Threading.Tasks.Dataflow;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;

public class MainMapManager : MonoBehaviour
{
    public static MainMapManager Instance { get; private set; }
    public Transform mapContainer;
    private GameObject currentMapInstance;
    //public List<GameObject> mapInstances = new List<GameObject>();
    public GameObject TeleportUI;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void OnDestroy()
    {
        if (currentMapInstance != null)
            Addressables.ReleaseInstance(currentMapInstance);

        Debug.Log("체크용");
    }

    public void ChangeMap(string newMapAddress, string playerSpawnPosition)
    {
        // FadeManager에게 화면을 어둡게 하라고 요청
        FadeManager.Instance.FadeOut(() =>
        {
            StartCoroutine(ProcessMapChange(newMapAddress, playerSpawnPosition));
        });
    }
    private IEnumerator ProcessMapChange(string newMapAddress, string playerSpawnPosition)
    {
        // 1. 이전 맵 및 오브젝트 풀 정리 (메모리 확보 핵심)
        if (currentMapInstance != null)
        {
            // 오브젝트 풀에 남아있는 몬스터들 제거
            //ObjectPoolingManager.instance.ClearAllPools();
            
            // Addressables를 통해 생성된 인스턴스 해제
            Addressables.ReleaseInstance(currentMapInstance);
            currentMapInstance = null;
        }

        // 2. 새 맵 로드 (주소 기반)
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(newMapAddress, mapContainer);
        yield return handle; // 로드 완료까지 대기

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            currentMapInstance = handle.Result;
            currentMapInstance.name = newMapAddress;

            // 3. 플레이어 이동
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                TeleportManager.Instance.TeleportPlayer(player, newMapAddress, playerSpawnPosition);
            }
        }

        // 4. 화면 밝게 하기
        FadeManager.Instance.FadeIn();
    }
    public void InitializeMap(string newMapAddress, string playerSpawnPosition)
    {
         StartCoroutine(ProcessMapChange(newMapAddress, playerSpawnPosition));
    }
    
    public GameObject GetTeleportUI()
    {
        return TeleportUI;
    }
}
