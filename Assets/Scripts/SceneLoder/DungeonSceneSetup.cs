using System.Collections;
using UnityEngine;

public class DungeonSceneSetup : MonoBehaviour, ISceneInitializer
{
    public QuestUIManager questUIManager;
    public IEnumerator Initialize()
    {
        Debug.Log("던전 씬 초기화 시작...");
        if (!AreManagersReady())
        {
            yield break;
        }

        EnCounterSystem.Instance.InitializeSceneComponents();

        TeleportManager.Instance.SetInitialSpawnPoint();

        PlayerStats.Instance.init();
        CharacterSelection characterSelection = GameManager.Instance.GetComponent<CharacterSelection>();
        if (characterSelection != null)
        {
            characterSelection.OnceSetting();
        }
        else
        {
            Debug.LogWarning("[DungeonSceneSetup] CharacterSelection is missing on GameManager.");
        }
        GameManager.Instance.SearchPlayer();
        
        ObjectPoolingManager.Instance.PrewarmDamageText(100);
        
        InventorySystem.InventoryController.instance.init();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerHealth playerHealth = player != null ? player.GetComponent<PlayerHealth>() : null;
        if (playerHealth != null) playerHealth.InitializeForNewScene();

        Resources.UnloadUnusedAssets();

        if (questUIManager != null) questUIManager.RefreshAllQuestUI();
        yield return new WaitForSeconds(0.5f); // 연출용 대기
    }

    private bool AreManagersReady()
    {
        bool isReady = true;

        if (EnCounterSystem.Instance == null)
        {
            Debug.LogError("[DungeonSceneSetup] EnCounterSystem is missing.");
            isReady = false;
        }
        if (TeleportManager.Instance == null)
        {
            Debug.LogError("[DungeonSceneSetup] TeleportManager is missing.");
            isReady = false;
        }
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("[DungeonSceneSetup] PlayerStats is missing.");
            isReady = false;
        }
        if (GameManager.Instance == null)
        {
            Debug.LogError("[DungeonSceneSetup] GameManager is missing.");
            isReady = false;
        }
        if (ObjectPoolingManager.Instance == null)
        {
            Debug.LogError("[DungeonSceneSetup] ObjectPoolingManager is missing.");
            isReady = false;
        }
        if (InventorySystem.InventoryController.instance == null)
        {
            Debug.LogError("[DungeonSceneSetup] InventoryController is missing.");
            isReady = false;
        }

        return isReady;
    }
}
