using System.Collections;
using UnityEngine;

public class DungeonSceneSetup : MonoBehaviour, ISceneInitializer
{
    public QuestUIManager questUIManager;
    public IEnumerator Initialize()
    {
        Debug.Log("던전 씬 초기화 시작...");
        EnCounterSystem.Instance.InitializeSceneComponents();

        TeleportManager.Instance.SetInitialSpawnPoint();

        PlayerStats.Instance.init();
        GameManager.Instance.GetComponent<CharacterSelection>().OnceSetting();
        GameManager.Instance.SearchPlayer();
        
        ObjectPoolingManager.Instance.PrewarmDamageText(100);
        
        InventorySystem.InventoryController.instance.init();

        GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerHealth>().InitializeForNewScene();

        Resources.UnloadUnusedAssets();

        questUIManager.RefreshAllQuestUI();
        yield return new WaitForSeconds(0.5f); // 연출용 대기
    }
}