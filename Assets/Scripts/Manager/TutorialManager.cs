using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using InventorySystem;

// 튜토리얼의 현재 진행 상태를 나타내는 상태기(State Machine)
public enum TutorialStep
{
    None,
    TownPhase1_Intro,     // 첫 마을 진입
    TownPhase1_GoBattle,  // 전투하러 가기 버튼 유도
    BattlePhase_Attack,   // 전투 씬 진입 및 튜토리얼
    TownPhase2_Upgrade,   // 전투 후 마을로 돌아와서 강화 유도
    Complete              // 완료 및 자폭
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("현재 튜토리얼 상태")]
    public TutorialStep currentStep = TutorialStep.None;

    [Header("UI 시스템 (프리팹 내부에 세팅)")]
    public GameObject tutorialCanvas;      // 튜토리얼 전용 캔버스 (Sorting Order를 최상단으로 설정할 것)
    public Image blockerPanel;             // 화면 전체를 가리는 반투명 검은색 패널 (Raycast Target ON)
    public GameObject dialogPanel;
    public TMP_Text dialogText;            // 대사 텍스트
    public Button nextDialogButton;        // 대사 넘기기 버튼 (화면 전체 크기로 투명하게 두면 화면 터치로 넘어감)
    public RectTransform guidePointer; 
    public float animSpeed = 5f;
    public float maxScale = 1.1f;
    public float pointerBounceHeight = 20f;
    [Header("듀토리얼 버튼 캐싱")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button characterButton;
    [SerializeField] private Button characterConfirmButton;
    [SerializeField] private Button forestZoneButton;
    [SerializeField] private Button forestPointButton;
    [SerializeField] private Button teleportConfirmButton;

    [Header("씬 전환 후 다시 찾을 버튼 이름")]
    [SerializeField] private string inventoryButtonName = "Inventory";
    [SerializeField] private string enchantButtonName = "Enchant Up button";
    [SerializeField] private string inventoryBackButtonName = "Inventory Back";
    [SerializeField] private string upgradeButtonName = "Upgrade";
    [SerializeField, Min(0.1f)] private float buttonSearchTimeout = 10f;

    [SerializeField] private string VillageSceneName = "Village";
    [SerializeField] private string IngameSceneName = "GameplayScene";
    [SerializeField] private string forestZoneButtonName = "숲";
    [SerializeField] private string forestPointButtonName = "숲 1-1";

    private Button inventoryButton;
    private Button enchantButton;
    private Button inventoryBackButton;
    private Button upgradeButton;

    private Coroutine activeTutorialRoutine;
    private Coroutine activeHighlightRoutine;
    private UnityEngine.Events.UnityAction activeDialogClickAction;

    private Transform highlightedTarget;
    private Transform highlightedOriginalParent;
    private int highlightedOriginalSiblingIndex;
    private Button highlightedButton;
    private UnityEngine.Events.UnityAction highlightedButtonClickAction;
    private Slot highlightedSlot;
    private System.Action highlightedSlotClickAction;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        // 내가 현재 활성화된 인스턴스였다면, 파괴될 때 참조를 비워줌
        if (Instance == this)
        {
            StopActiveTutorialRoutine();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
    public void CheckTutorial()
    {
        //GameProgressManager.Instance.Dislock("Tutorial");
        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[TutorialManager] GameProgressManager is missing.");
            return;
        }

        if (GameProgressManager.Instance.IsUnlocked("Tutorial"))
        {
            // 나 자신(스크립트)이 아니라, 나를 포함한 이 거대한 UI 오브젝트 덩어리 전체를 즉시 파괴!
            Destroy(gameObject); 
            return;
        }

        StartTutorial();
    }
    public void StartTutorial()
    {
        if (currentStep == TutorialStep.None)
        {
            // 이벤트(OnSceneLoaded)를 기다리지 않고, 태어나자마자 1부를 강제로 시작!
            StartTutorialRoutine(TownPhase1_Routine());//TownPhase1_Routine
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 이미 튜토리얼이 끝났다면 무시
        if (currentStep == TutorialStep.Complete) return;

        if (scene.name == VillageSceneName)
        {
            if (currentStep == TutorialStep.BattlePhase_Attack)
            {
                // 전투가 끝나고 다시 마을로 옴: 마을 튜토리얼 2부
                StartTutorialRoutine(TownPhase2_Routine());
            }
        }
        else if (scene.name == IngameSceneName)
        {
            if (currentStep == TutorialStep.TownPhase1_GoBattle)
            {
                // 전투 씬 최초 진입
                DevLog.Log("호출타이밍 체크");
                StartTutorialRoutine(BattlePhase_Routine());
            }
        }
    }

    private void StartTutorialRoutine(IEnumerator routine)
    {
        StopActiveTutorialRoutine();
        activeTutorialRoutine = StartCoroutine(RunTutorialRoutine(routine));
    }

    private IEnumerator RunTutorialRoutine(IEnumerator routine)
    {
        yield return routine;
        activeTutorialRoutine = null;
    }

    private void StopActiveTutorialRoutine()
    {
        if (activeTutorialRoutine != null)
        {
            StopCoroutine(activeTutorialRoutine);
            activeTutorialRoutine = null;
        }

        ClearDialogClickListener();
        RestoreHighlightedTarget();
    }

    private void ClearDialogClickListener()
    {
        if (nextDialogButton != null && activeDialogClickAction != null)
        {
            nextDialogButton.onClick.RemoveListener(activeDialogClickAction);
        }

        activeDialogClickAction = null;

        if (nextDialogButton != null)
        {
            nextDialogButton.gameObject.SetActive(false);
        }
    }

    private void CaptureHighlightedTarget(Transform target)
    {
        RestoreHighlightedTarget();

        highlightedTarget = target;
        highlightedOriginalParent = target.parent;
        highlightedOriginalSiblingIndex = target.GetSiblingIndex();
    }

    private void RestoreHighlightedTarget()
    {
        if (activeHighlightRoutine != null)
        {
            StopCoroutine(activeHighlightRoutine);
            activeHighlightRoutine = null;
        }

        if (highlightedButton != null && highlightedButtonClickAction != null)
        {
            highlightedButton.onClick.RemoveListener(highlightedButtonClickAction);
        }

        if (highlightedSlot != null && highlightedSlotClickAction != null)
        {
            highlightedSlot.OnSlotClickedForTutorial -= highlightedSlotClickAction;
        }

        if (guidePointer != null)
        {
            guidePointer.gameObject.SetActive(false);
        }

        if (highlightedTarget != null)
        {
            highlightedTarget.localScale = Vector3.one;

            if (highlightedOriginalParent != null)
            {
                highlightedTarget.SetParent(highlightedOriginalParent);
                highlightedTarget.SetSiblingIndex(highlightedOriginalSiblingIndex);
            }
        }

        highlightedTarget = null;
        highlightedOriginalParent = null;
        highlightedOriginalSiblingIndex = 0;
        highlightedButton = null;
        highlightedButtonClickAction = null;
        highlightedSlot = null;
        highlightedSlotClickAction = null;
    }

    private IEnumerator TownPhase1_Routine()
    {
        currentStep = TutorialStep.TownPhase1_Intro;
        tutorialCanvas.SetActive(true);
        blockerPanel.gameObject.SetActive(true); // 모든 터치 방지 시작

        yield return ShowDialog("용사님, 깨어나셨군요! 마을에 오신 것을 환영합니다.");
        yield return ShowDialog("우선 몸을 풀기 위해 전투를 한 번 진행해볼까요?");

        currentStep = TutorialStep.TownPhase1_GoBattle;
        

        yield return HighlightButtonAndWait(startButton, "중앙의 [전투 입장] 버튼을 눌러주세요!");

        yield return null;//1프레임 대기

        yield return HighlightButtonAndWait(characterButton, "캐릭터를 눌러주세요!");
        yield return null;//1프레임 대기

        yield return HighlightButtonAndWait(characterConfirmButton, "캐릭터를 눌러주세요!");
        yield return null;//1프레임 대기


        yield return WaitForButtonByName(forestZoneButtonName, button => forestZoneButton = button);
        yield return HighlightButtonAndWait(forestZoneButton, "듀토리얼 지역 버튼을 눌러주세요!");
    
        yield return null;//1프레임 대기

        

        yield return WaitForButtonByName(forestPointButtonName, button => forestPointButton = button);
        yield return HighlightButtonAndWait(forestPointButton, "듀토리얼 지역 버튼을 눌러주세요!");
        
        yield return null;//1프레임 대기
        
        yield return HighlightButtonAndWait(teleportConfirmButton, "이동 버튼을 눌러주세요!");
        // 버튼이 눌리면 씬이 전환될 것이므로, 여기서는 캔버스만 꺼줍니다.
        tutorialCanvas.SetActive(false);
    }

    private IEnumerator BattlePhase_Routine()
    {
        currentStep = TutorialStep.BattlePhase_Attack;
        tutorialCanvas.SetActive(true);
        blockerPanel.gameObject.SetActive(true);
        GameObject playerObj = GameObject.FindWithTag("Player");
        Player_Controller playerMove = null;
        if (playerObj != null)
        {
            playerMove = playerObj.GetComponent<Player_Controller>();
            
            if (playerMove != null) playerMove.StopMoving = true;
        }

        yield return ShowDialog("전투는 기본적으로 인카운트를 통해 발생합니다.");
        yield return ShowDialog("위의 검은 네모칸 안에서 돌아다녀보세요");
        yield return ShowDialog("최대 인카운트 횟수는 지도 아래 적혀있습니다.");
        yield return ShowDialog("이동은 WASD를 통해 가능하고 SPACE BAR를 통해 대쉬가 가능합니다.");
        yield return ShowDialog("대쉬 쿨타임은 왼쪽 위 HP바 아래에 존재합니다.");

        EnCounterSystem.Instance.maxEncounter = 1;

        blockerPanel.gameObject.SetActive(false); 
        dialogText.transform.parent.gameObject.SetActive(false);

        if (playerMove != null) playerMove.StopMoving = false;
    }

    private IEnumerator TownPhase2_Routine()
    {
        currentStep = TutorialStep.TownPhase2_Upgrade;
        ClearTownPhase2ButtonCache();

        tutorialCanvas.SetActive(true);
        blockerPanel.gameObject.SetActive(true);
        dialogText.transform.parent.gameObject.SetActive(true); 

        yield return ShowDialog("무사히 돌아오셨군요! 방금 얻은 재화로 무기를 강화해보세요.");

        // 상점 버튼 찾아서 클릭 유도
        yield return WaitForButtonByName(inventoryButtonName, button => inventoryButton = button);
        yield return HighlightButtonAndWait(inventoryButton, "[인벤토리] 버튼을 눌러보세요.");
        yield return null;

        Slot[] allSlots = Object.FindObjectsByType<Slot>(FindObjectsSortMode.None);
        Slot targetSlot = allSlots.FirstOrDefault(s => 
                                                        s.GetPosition() == 0 && 
                                                        s.slotType == "Inventory"
                                                    );

        if (targetSlot != null)
        {
            yield return HighlightSlotAndWait(targetSlot, "첫 번째 아이템을 눌러 장착하세요!");
        }
        yield return null;
        
        yield return WaitForButtonByName(enchantButtonName, button => enchantButton = button);
        yield return HighlightButtonAndWait(enchantButton, "[강화] 버튼을 눌러보세요.");
        yield return null;

        yield return ShowDialog("아이템을 더블클릭하면 장착이 가능하고");
        yield return ShowDialog("장착된 아이템을 더블클릭하면 장착이 해제됩니다.");

        yield return WaitForButtonByName(inventoryBackButtonName, button => inventoryBackButton = button);
        yield return HighlightButtonAndWait(inventoryBackButton, "[뒤로가기] 버튼을 눌러보세요.");
        yield return null;
        
        yield return WaitForButtonByName(upgradeButtonName, button => upgradeButton = button);
        yield return HighlightButtonAndWait(upgradeButton, "[업그레이드] 버튼을 눌러보세요.");
        yield return null;

        yield return ShowDialog("다양한 등급별 강화가 존재하고");
        yield return ShowDialog("이전 단계의 모든 업그레이드를 완료해야 다음 단계를 진행할 수 있습니다.");
        yield return ShowDialog("아이템을 줍고, 강화하고 , 스탯 업그레이드를 해보세요");
        yield return ShowDialog("점점 더 높은 사냥터에서 싸움을 시작할 수 있게되게 될 것입니다.");
        yield return ShowDialog("아이템의 경우 장착해제불가 아이템이 아닌이상 드랍한 아이템으로 갈아입는것이 가능합니다.");
        yield return ShowDialog("완벽합니다! 이제 모든 준비가 끝났습니다. 행운을 빕니다!");

        EndTutorialAndDestroy();
    }

    private IEnumerator ShowDialog(string text, bool closeAfter = false)
    {
        ClearDialogClickListener();

        dialogPanel.SetActive(true); 

        dialogText.text = text;
        bool isClicked = false;

        // 대사 넘기기 버튼(화면 전체 투명 버튼)에 임시 이벤트 등록
        activeDialogClickAction = () => { isClicked = true; };
        nextDialogButton.onClick.AddListener(activeDialogClickAction);
        nextDialogButton.gameObject.SetActive(true);

        // 유저가 화면을 터치할 때까지 대기
        yield return new WaitUntil(() => isClicked);

        // 이벤트 해제
        ClearDialogClickListener();

        if (closeAfter)
        {
            dialogPanel.SetActive(false);
        }
    }

    private IEnumerator HighlightButtonAndWait(Button targetButton, string guideText)
    {
        if (targetButton == null)
        {
            Debug.LogWarning($"[TutorialManager] Tutorial target button is missing. Guide text: {guideText}");
            yield break;
        }

        dialogText.text = guideText;

        CaptureHighlightedTarget(targetButton.transform);
        highlightedButton = targetButton;

        // 타겟 버튼을 차단막(blockerPanel)의 자식으로 잠시 이동시켜서 화면 맨 위로 끌어올림!
        targetButton.transform.SetParent(blockerPanel.transform);
        targetButton.transform.SetAsLastSibling(); 

        activeHighlightRoutine = StartCoroutine(HighlightAnimationRoutine(targetButton.transform));

        // 버튼 클릭 이벤트 추적
        bool isTargetClicked = false;
        highlightedButtonClickAction = () => { isTargetClicked = true; };
        targetButton.onClick.AddListener(highlightedButtonClickAction);

        // 클릭할 때까지 무한 대기
        yield return new WaitUntil(() => isTargetClicked || targetButton == null);

        RestoreHighlightedTarget();
    }
    private IEnumerator HighlightSlotAndWait(Slot targetSlot, string guideText)
    {
        if (targetSlot == null)
        {
            Debug.LogWarning($"[TutorialManager] Tutorial target slot is missing. Guide text: {guideText}");
            yield break;
        }

        dialogText.text = guideText;

        CaptureHighlightedTarget(targetSlot.transform);
        highlightedSlot = targetSlot;

        targetSlot.transform.SetParent(blockerPanel.transform);
        targetSlot.transform.SetAsLastSibling(); 

        activeHighlightRoutine = StartCoroutine(HighlightAnimationRoutine(targetSlot.transform));

        bool isTargetClicked = false;
        highlightedSlotClickAction = () => { isTargetClicked = true; };
        
        targetSlot.OnSlotClickedForTutorial += highlightedSlotClickAction;

        yield return new WaitUntil(() => isTargetClicked || targetSlot == null);

        RestoreHighlightedTarget();
    }

    private IEnumerator HighlightAnimationRoutine(Transform targetTransform)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = new Vector3(maxScale, maxScale, 1f);

        Vector3 pointerStartPos = Vector3.zero;
        if (guidePointer != null)
        {
            guidePointer.gameObject.SetActive(true);
            guidePointer.SetAsLastSibling(); // 화살표도 맨 앞으로

            guidePointer.position = targetTransform.position + new Vector3(0, 50f, 0);
            pointerStartPos = guidePointer.position;
        }

        float timer = 0f;

        while (true)
        {
            if (targetTransform == null)
            {
                yield break;
            }

            timer += Time.deltaTime * animSpeed;

            float scaleLerp = Mathf.PingPong(timer, 1f);
            targetTransform.localScale = Vector3.Lerp(originalScale, targetScale, scaleLerp);

            if (guidePointer != null)
            {
                float bounceOffset = Mathf.Sin(timer * 2f) * pointerBounceHeight;
                guidePointer.position = pointerStartPos + new Vector3(0, bounceOffset, 0);
            }

            yield return null;
        }
    }
    private IEnumerator WaitForButtonByName(string objectName, System.Action<Button> onFound)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            onFound?.Invoke(null);
            yield break;
        }

        Button button = null;
        float elapsed = 0f;

        while (button == null && elapsed < buttonSearchTimeout)
        {
            GameObject obj = GameObject.Find(objectName);
            if (obj != null)
                button = obj.GetComponent<Button>();

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (button == null)
            Debug.LogWarning($"[TutorialManager] Could not find tutorial button named '{objectName}'.");

        onFound?.Invoke(button);
    }

    private void ClearTownPhase2ButtonCache()
    {
        inventoryButton = null;
        enchantButton = null;
        inventoryBackButton = null;
        upgradeButton = null;
    }

    private void EndTutorialAndDestroy()
    {
        currentStep = TutorialStep.Complete;
        activeTutorialRoutine = null;
        ClearDialogClickListener();
        RestoreHighlightedTarget();

        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(false);
        }

        // 유저 데이터에 튜토리얼 클리어 저장
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.Unlock("Tutorial");
        }

        // 씬 로드 이벤트 구독 취소
        SceneManager.sceneLoaded -= OnSceneLoaded;

        DevLog.Log("튜토리얼 완료! 매니저를 파괴합니다.");

        Destroy(gameObject);
    }
}
