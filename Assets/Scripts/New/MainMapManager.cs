using System.Collections;
using UnityEngine;

public class MainMapManager : MonoBehaviour
{
    public static MainMapManager Instance { get; private set; }
    
    public Transform mapContainer;
    private GameObject currentMapInstance;
    public GameObject TeleportUI;
    
    private string currentMapTheme = "";
    
    // 메모리 해제를 위해 현재 맵의 '문자열 주소'를 기억해둡니다.
    private string currentMapAddress = ""; 

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
    
    public void OnDestroy()
    {
        // 씬이 파괴될 때 맵 인스턴스 파괴 및 메모리 해제 지시
        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
            if (!string.IsNullOrEmpty(currentMapAddress))
            {
                ResourceManager.Instance.UnloadAssetByKey(currentMapAddress);
            }
        }
        Instance = null;
    }

    public void ChangeMap(string newMapAddress, string playerSpawnPosition)
    {
        FadeManager.Instance.FadeOut(() =>
        {
            StartCoroutine(ProcessMapChange(newMapAddress, playerSpawnPosition));
        });
    }

    private IEnumerator ProcessMapChange(string newMapAddress, string playerSpawnPosition)
    {
        string[] nameParts = newMapAddress.Split(' ');
        string newTheme = (nameParts.Length > 0) ? nameParts[0] : newMapAddress;

        bool isMapLoaded = false;
        GameObject mapPrefab = null;

        //  ResourceManager에게 맵 프리팹을 메모리에 올려달라고 지시
        ResourceManager.Instance.LoadAssetByKey(newMapAddress, (loadedPrefab) => 
        {
            mapPrefab = loadedPrefab;
            isMapLoaded = true;
        });

        // 리소스 매니저가 메모리에 맵을 다 올릴 때까지 대기
        yield return new WaitUntil(() => isMapLoaded);

        if (mapPrefab != null)
        {
            // 이전 맵 및 오브젝트 풀 정리
            if (currentMapInstance != null)
            {
                Destroy(currentMapInstance);
                ResourceManager.Instance.UnloadAssetByKey(currentMapAddress);
            }

            if (currentMapTheme != "" && currentMapTheme != newTheme)
            {
                ObjectPoolingManager.instance.ClearAllPools();
                if (WaveSpawner.Instance != null) WaveSpawner.Instance.ReleaseWaveAssets();
            }
            else
            {
                ObjectPoolingManager.instance.ReturnAllActiveObjectsToPool();
            }

            currentMapTheme = newTheme;

            // 메모리에 올라온 프리팹을 유니티 기본 Instantiate로 맵 생성
            currentMapInstance = Instantiate(mapPrefab, mapContainer);
            currentMapInstance.name = newMapAddress;
            currentMapAddress = newMapAddress; // 다음 맵 이동 시 해제할 수 있게 주소 기억
            currentMapInstance.SetActive(true);

            //플레이어 이동 및 마무리
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