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
    private string currentMapTheme = "";

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

        Instance = null;
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
        string[] nameParts = newMapAddress.Split(' ');
        string newTheme = (nameParts.Length > 0) ? nameParts[0] : newMapAddress;

        var handle = Addressables.InstantiateAsync(newMapAddress, mapContainer);
        yield return handle;
        //이전 맵 및 오브젝트 풀 정리
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject newMap = handle.Result;
            newMap.SetActive(false); // 잠시 꺼둠 (초기화 등을 위해)

            // 기존 맵 제거 (이제 제거해도 번들은 newMap 때문에 메모리에 남음)
            if (currentMapInstance != null)
            {
                Addressables.ReleaseInstance(currentMapInstance);
            }
            if(currentMapTheme != "" && currentMapTheme != newTheme)
            {
                ObjectPoolingManager.instance.ClearAllPools();
                WaveSpawner.Instance.ReleaseWaveAssets();
            }
            else
            {
                ObjectPoolingManager.instance.ReturnAllActiveObjectsToPool();
            }

            currentMapTheme = newTheme;

            // 교체 및 설정
            currentMapInstance = newMap;
            currentMapInstance.name = newMapAddress;
            currentMapInstance.SetActive(true);

            // 플레이어 이동 및 마무리
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                TeleportManager.Instance.TeleportPlayer(player, newMapAddress, playerSpawnPosition);
            }
            
            FadeManager.Instance.FadeIn();
        }
        else
        {
            Debug.LogError("맵 로드 실패: " + newMapAddress);
        }
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
