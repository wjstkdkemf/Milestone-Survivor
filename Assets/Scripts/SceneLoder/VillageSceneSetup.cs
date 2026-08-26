using UnityEngine;
using System.Collections;
using InventorySystem;
public class VillageSceneSetup : MonoBehaviour, ISceneInitializer
{
    public AudioClip MainBGM;
    public QuestUIManager questUIManager;
    public IEnumerator Initialize()
    {
        string prevScene = LoadingManager.Instance != null ? LoadingManager.Instance.PreviousSceneName : "";
        Debug.Log($"마을 씬 초기화 시작 (이전 씬: {prevScene})");

        // 공통 초기화 (어디서 왔든 무조건 해야 하는 것)
        // 예: UI 켜기, 기본 BGM 재생
        // UIManager.Instance.ShowHUD();
        
        // 분기 처리
        if (prevScene == "Main Menu 1" || string.IsNullOrEmpty(prevScene))
        {
            // [케이스 1] 게임을 처음 켰을 때 (타이틀 -> 마을)
            yield return StartCoroutine(InitFromTitle());
        }
        else if (prevScene == "GameplayScene") // 던전 씬 이름 확인 필요
        {
            // [케이스 2] 던전을 돌고 복귀했을 때 (던전 -> 마을)
            yield return StartCoroutine(InitFromDungeon());
        }
        else
        {
            // 그 외 (상점 등에서 왔을 때)
            Debug.Log("일반 복귀 초기화 수행");
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(MainBGM);
        }
        else
        {
            Debug.LogWarning("[VillageSceneSetup] AudioManager is missing. BGM will not play.");
        }

        yield return null;
    }

    // 게임 시작 시 초기화 (데이터 로드 위주)
    private IEnumerator InitFromTitle()
    {
        Debug.Log(">>> 타이틀 화면에서 진입: 저장된 데이터 로드 중...");

        if (!AreTitleManagersReady())
        {
            yield break;
        }

        TeleportManager.Instance.LoadData();
        GameProgressManager.Instance.LoadProgress();
        InventoryController.instance.init();

        Resources.UnloadUnusedAssets();

        if (questUIManager != null) questUIManager.RefreshAllQuestUI();
        if (TutorialManager.Instance != null) TutorialManager.Instance.CheckTutorial();


        yield return new WaitForSeconds(0.5f); // 연출용 딜레이
    }

    // 던전 복귀 시 초기화 (정산 및 저장 위주)
    private IEnumerator InitFromDungeon()
    {
        Debug.Log(">>> 던전에서 복귀: 전리품 정산 및 자동 저장 중...");

        if (InventoryController.instance == null)
        {
            Debug.LogError("[VillageSceneSetup] InventoryController is missing. Cannot initialize dungeon return.");
            yield break;
        }

        InventoryController.instance.init();

        if (LoadScreenManager.Instance != null)
        {
            LoadScreenManager.Instance.ConfirmSelectionSave();
        }
        else
        {
            Debug.LogWarning("[VillageSceneSetup] LoadScreenManager is missing. Auto-save skipped.");
        }

        if (questUIManager != null) questUIManager.RefreshAllQuestUI();

        yield return new WaitForSeconds(0.5f); // 연출용 딜레이
    }

    private bool AreTitleManagersReady()
    {
        bool isReady = true;

        if (TeleportManager.Instance == null)
        {
            Debug.LogError("[VillageSceneSetup] TeleportManager is missing.");
            isReady = false;
        }
        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("[VillageSceneSetup] GameProgressManager is missing.");
            isReady = false;
        }
        if (InventoryController.instance == null)
        {
            Debug.LogError("[VillageSceneSetup] InventoryController is missing.");
            isReady = false;
        }

        return isReady;
    }
}
