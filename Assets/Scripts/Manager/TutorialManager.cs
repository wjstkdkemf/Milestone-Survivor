using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
    
    // 유저가 클릭해야 할 타겟 버튼을 임시로 기억해둘 변수
    private Transform originalTargetParent;
    private int originalTargetSiblingIndex;

    private void Awake()
    {
        if (GameProgressManager.Instance.IsUnlocked("Tutorial"))
        {
            // 나 자신(스크립트)이 아니라, 나를 포함한 이 거대한 UI 오브젝트 덩어리 전체를 즉시 파괴!
            Destroy(gameObject); 
            return; // 아래의 싱글톤 등록 로직조차 실행하지 않고 함수 강제 종료
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 사장님 기획대로 다중 씬 유지를 위해 DDOL 적용!
            
            // 씬이 변경될 때마다 OnSceneLoaded 함수가 자동으로 실행되도록 구독(구독)합니다.
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
            Instance = null;
        }
    }
    public void StartTutorial()
    {
        if (currentStep == TutorialStep.None)
        {
            // 이벤트(OnSceneLoaded)를 기다리지 않고, 태어나자마자 1부를 강제로 시작!
            StartCoroutine(TownPhase1_Routine());
        }
    }

    // [핵심] 씬이 바뀔 때마다 파괴되지 않고 이 함수가 실행됩니다!
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 이미 튜토리얼이 끝났다면 무시
        if (currentStep == TutorialStep.Complete) return;

        // 씬 이름에 따라 튜토리얼 페이즈를 이어갑니다.
        if (scene.name == "Viliage")
        {
            if (currentStep == TutorialStep.None)
            {
                // 게임 최초 시작: 마을 튜토리얼 1부
                StartCoroutine(TownPhase1_Routine());
            }
            else if (currentStep == TutorialStep.BattlePhase_Attack)
            {
                // 전투가 끝나고 다시 마을로 옴: 마을 튜토리얼 2부
                StartCoroutine(TownPhase2_Routine());
            }
        }
        else if (scene.name == "BattleScene")
        {
            if (currentStep == TutorialStep.TownPhase1_GoBattle)
            {
                // 전투 씬 최초 진입
                StartCoroutine(BattlePhase_Routine());
            }
        }
    }

    // =====================================================================
    // STEP 1: 마을 튜토리얼 1부 (첫 접속)
    // =====================================================================
    private IEnumerator TownPhase1_Routine()
    {
        currentStep = TutorialStep.TownPhase1_Intro;
        tutorialCanvas.SetActive(true);
        blockerPanel.gameObject.SetActive(true); // 모든 터치 방지 시작

        // 1. 단순 대화 진행
        yield return ShowDialog("용사님, 깨어나셨군요! 마을에 오신 것을 환영합니다.");
        yield return ShowDialog("우선 몸을 풀기 위해 전투를 한 번 진행해볼까요?");

        // 2. 특정 버튼 클릭 유도 (예: "전투 입장" 버튼)
        currentStep = TutorialStep.TownPhase1_GoBattle;
        
        // 씬에서 "전투 입장" 버튼을 이름이나 태그로 찾습니다. (DDOL이므로 인스펙터 연결 불가)
        GameObject battleBtnObj = GameObject.Find("GoBattleButton"); 
        
        if (battleBtnObj != null)
        {
            Button battleBtn = battleBtnObj.GetComponent<Button>();
            yield return HighlightButtonAndWait(battleBtn, "우측 하단의 [전투 입장] 버튼을 눌러주세요!");
        }

        // 버튼이 눌리면 씬이 전환될 것이므로, 여기서는 캔버스만 꺼줍니다.
        tutorialCanvas.SetActive(false);
    }

    // =====================================================================
    // STEP 2: 전투 씬 튜토리얼
    // =====================================================================
    private IEnumerator BattlePhase_Routine()
    {
        currentStep = TutorialStep.BattlePhase_Attack;
        tutorialCanvas.SetActive(true);
        blockerPanel.gameObject.SetActive(true);

        yield return ShowDialog("전투에 진입했습니다!");
        yield return ShowDialog("가까이 다가가면 자동으로 공격합니다. 적을 모두 처치하세요!");

        // 조작을 해야 하므로 차단막 해제 및 대화창 숨김
        blockerPanel.gameObject.SetActive(false); 
        dialogText.transform.parent.gameObject.SetActive(false); 

        // 튜토리얼 전용 몬스터 소환 등 로직 진행...
        // 전투가 끝나고 다시 마을 씬으로 로딩될 때까지 매니저는 조용히 잠복합니다.
    }

    // =====================================================================
    // STEP 3: 마을 튜토리얼 2부 (전투 후 복귀 및 자폭)
    // =====================================================================
    private IEnumerator TownPhase2_Routine()
    {
        currentStep = TutorialStep.TownPhase2_Upgrade;
        tutorialCanvas.SetActive(true);
        blockerPanel.gameObject.SetActive(true);
        dialogText.transform.parent.gameObject.SetActive(true); 

        yield return ShowDialog("무사히 돌아오셨군요! 방금 얻은 재화로 무기를 강화해보세요.");

        // 상점 버튼 찾아서 클릭 유도
        GameObject shopBtnObj = GameObject.Find("ShopButton");
        if (shopBtnObj != null)
        {
            Button shopBtn = shopBtnObj.GetComponent<Button>();
            yield return HighlightButtonAndWait(shopBtn, "[상점] 버튼을 눌러보세요.");
        }

        yield return ShowDialog("완벽합니다! 이제 모든 준비가 끝났습니다. 행운을 빕니다!");

        // 대장정의 마무리: 매니저 자폭
        EndTutorialAndDestroy();
    }

    // =====================================================================
    // 공통 헬퍼 함수 1: 대사 출력하고 터치 대기
    // =====================================================================
    private IEnumerator ShowDialog(string text, bool closeAfter = false)
    {
        dialogPanel.SetActive(true); 

        dialogText.text = text;
        bool isClicked = false;

        // 대사 넘기기 버튼(화면 전체 투명 버튼)에 임시 이벤트 등록
        UnityEngine.Events.UnityAction onClickAction = () => { isClicked = true; };
        nextDialogButton.onClick.AddListener(onClickAction);
        nextDialogButton.gameObject.SetActive(true);

        // 유저가 화면을 터치할 때까지 대기 (CPU 소모 X)
        yield return new WaitUntil(() => isClicked);

        // 이벤트 해제
        nextDialogButton.onClick.RemoveListener(onClickAction);
        nextDialogButton.gameObject.SetActive(false);

        if (closeAfter)
        {
            dialogPanel.SetActive(false);
        }
    }

    // =====================================================================
    // 공통 헬퍼 함수 2: [핵심] 터치 차단막 위로 타겟 버튼 구출하기
    // =====================================================================
    private IEnumerator HighlightButtonAndWait(Button targetButton, string guideText)
    {
        dialogText.text = guideText;

        // 1. 타겟 버튼의 원래 위치 기억
        originalTargetParent = targetButton.transform.parent;
        originalTargetSiblingIndex = targetButton.transform.GetSiblingIndex();

        // 2. 타겟 버튼을 차단막(blockerPanel)의 자식으로 잠시 이동시켜서 화면 맨 위로 끌어올림!
        targetButton.transform.SetParent(blockerPanel.transform);
        targetButton.transform.SetAsLastSibling(); 

        // 3. 버튼 클릭 이벤트 추적
        bool isTargetClicked = false;
        UnityEngine.Events.UnityAction onClickAction = () => { isTargetClicked = true; };
        targetButton.onClick.AddListener(onClickAction);

        // 4. 클릭할 때까지 무한 대기 (다른 화면 터치는 차단막이 다 씹어먹음)
        yield return new WaitUntil(() => isTargetClicked);

        // 5. 클릭 확인 후, 버튼을 원래 UI 계층으로 돌려놓기 및 이벤트 정리
        targetButton.transform.SetParent(originalTargetParent);
        targetButton.transform.SetSiblingIndex(originalTargetSiblingIndex);
        targetButton.onClick.RemoveListener(onClickAction);
    }

    // =====================================================================
    // 최종 자폭 시퀀스
    // =====================================================================
    private void EndTutorialAndDestroy()
    {
        currentStep = TutorialStep.Complete;
        tutorialCanvas.SetActive(false);

        // 유저 데이터에 튜토리얼 클리어 저장 (다음 앱 실행 시 안 나오게)
        GameProgressManager.Instance.Unlock("Tutorial");

        // 씬 로드 이벤트 구독 취소 (메모리 릭 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;

        Debug.Log("다중 씬 튜토리얼 대장정 완료! 매니저를 파괴합니다.");
        
        // 내 할 일은 끝났다. 스스로 파괴!
        Destroy(gameObject);
    }
}