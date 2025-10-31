using System.Collections;
using System.Collections.Generic;
//using System.Threading.Tasks.Dataflow;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class MainMapManager : MonoBehaviour
{
    public static MainMapManager Instance { get; private set; }
    public Transform mapContainer;
    private GameObject currentMapInstance;
    public List<GameObject> mapInstances = new List<GameObject>();
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

    public void ChangeMap(string newMapPrefab, string playerSpawnPosition)
    {
        // FadeManager에게 화면을 어둡게 하라고 요청
        FadeManager.Instance.FadeOut(() =>
        {
            GameObject foundObject = mapInstances.FirstOrDefault(obj => obj.name == newMapPrefab);

            // 1. 이전 맵이 있다면 파괴
            if (currentMapInstance != null && currentMapInstance.name != foundObject.name)
            {
                Destroy(currentMapInstance);
                currentMapInstance = Instantiate(foundObject, mapContainer);
            }
            else if(currentMapInstance == null)
            {
                currentMapInstance = Instantiate(foundObject, mapContainer);
            }
            // 2. 새 맵 프리팹을 mapContainer 하위에 생성
                
            // 3. 플레이어 위치 이동 (화면이 검은색이라 유저는 못 봄)
            // (Player 태그나 참조를 통해 플레이어 오브젝트를 찾아야 함)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                TeleportManager.Instance.TeleportPlayer(player, newMapPrefab ,playerSpawnPosition);
            }

            // 4. FadeManager에게 화면을 밝게 하라고 요청
            FadeManager.Instance.FadeIn();
        });
    }

    public GameObject GetTeleportUI()
    {
        return TeleportUI;
    }
}
