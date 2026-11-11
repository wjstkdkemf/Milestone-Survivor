using System.Collections;
using UnityEngine;

public class MainMapManager : MonoBehaviour
{
    public static MainMapManager Instance { get; private set; }
    
    public Transform mapContainer;
    private GameObject currentMapInstance;
    public GameObject TeleportUI;

    private bool isChangingMap = false;
    private int mapChangeVersion;
    private bool isLeavingScene;
    
    private string currentMapTheme = "";

    
    // 메모리 해제를 위해 현재 맵의 '문자열 주소'를 기억해둡니다.
    private string currentMapAddress = ""; 

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
    
    private void OnDestroy()
    {
        if (Instance != this) return;

        CancelForSceneExit();
        StopAllCoroutines();

        if (GameManager.Instance != null)
            GameManager.Instance.RequestPause(this, false);

        if (currentMapInstance != null)
            Destroy(currentMapInstance);

        Instance = null;
    }

    public void ChangeMap(string newMapAddress, string playerSpawnPosition)
    {
        if (isLeavingScene || isChangingMap) return;

        isChangingMap = true;
        int version = ++mapChangeVersion;
        FadeManager.Instance.FadeOut(() =>
        {
            if (!IsMapRequestValid(version)) return;
            StartCoroutine(ProcessMapChange(newMapAddress, playerSpawnPosition, version));
        });
    }
    private IEnumerator ProcessMapChange(string newMapAddress, string playerSpawnPosition, int version)
    {
        if (!IsMapRequestValid(version)) yield break;

        var gameManager = GameManager.Instance;

        try
        {
            string[] nameParts = newMapAddress.Split(' ');
            string newTheme = (nameParts.Length > 0) ? nameParts[0] : newMapAddress;

            bool isMapLoaded = false;
            GameObject mapPrefab = null;

            //  ResourceManager에게 맵 프리팹을 메모리에 올려달라고 지시
            ResourceManager.Instance.LoadAssetByKey(newMapAddress, (loadedPrefab) => 
            {
                if (!IsMapRequestValid(version)) return;
                mapPrefab = loadedPrefab;
                isMapLoaded = true;
            });

            // 리소스 매니저가 메모리에 맵을 다 올릴 때까지 대기
            yield return new WaitUntil(() => !IsMapRequestValid(version) || isMapLoaded);

            if (!IsMapRequestValid(version)) 
                yield break;

            if (mapPrefab != null)
            {
                bool themeChanged =
                    !string.IsNullOrEmpty(currentMapTheme) &&
                    currentMapTheme != newTheme;

                var poolManager = ObjectPoolingManager.Instance;
                var waveSpawner = WaveSpawner.Instance;

                gameManager.RequestPause(this, true);
                waveSpawner?.StopWaves();
                EnCounterSystem.Instance?.CancelEncounterForMapChange();

                // 테마 변경 여부와 관계없이 사용 중인 객체부터 반환한다.
                poolManager.ReturnAllActiveObjectsToPool();

                if (currentMapInstance != null)
                {
                    currentMapInstance.SetActive(false);
                    Destroy(currentMapInstance);
                    currentMapInstance = null;
                }

                if (themeChanged)
                    poolManager.ClearThemePools();

                // 풀 객체와 이전 맵의 Destroy 처리를 기다린다.
                yield return null;
                if (!IsMapRequestValid(version)) yield break;

                if (waveSpawner != null)
                {
                    yield return new WaitUntil(() =>
                        !IsMapRequestValid(version) || !waveSpawner.HasPendingWaveLoads);

                    if (!IsMapRequestValid(version)) yield break;

                    if (themeChanged)
                        waveSpawner.ReleaseWaveAssets();
                }

                // 같은 주소를 다시 로드한 경우 새 맵도 이 에셋을 사용한다.
                if (!string.IsNullOrEmpty(currentMapAddress) &&
                    currentMapAddress != newMapAddress)
                {
                    ResourceManager.Instance.UnloadAssetByKey(currentMapAddress);
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
            }
            else
            {
                Debug.LogError("맵 로드 실패: " + newMapAddress);
            }
        }
        finally
        {
            if (gameManager != null)
                gameManager.RequestPause(this, false);

            if (version == mapChangeVersion)
                isChangingMap = false;

            if (IsMapRequestValid(version) && FadeManager.Instance != null)
                FadeManager.Instance.FadeIn();
        }

    }

    public void InitializeMap(string newMapAddress, string playerSpawnPosition)
    {
        if (isLeavingScene || isChangingMap) return;

        isChangingMap = true;
        StartCoroutine(ProcessMapChange(newMapAddress, playerSpawnPosition, ++mapChangeVersion));
    }
    private bool IsMapRequestValid(int version)
    {
        return this != null && !isLeavingScene && version == mapChangeVersion;
    }

    public void CancelForSceneExit()
    {
        isLeavingScene = true;
        ++mapChangeVersion;
    }
    public GameObject GetTeleportUI()
    {
        return TeleportUI;
    }
    }