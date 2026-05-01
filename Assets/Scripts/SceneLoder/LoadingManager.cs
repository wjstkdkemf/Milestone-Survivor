using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    // [핵심 추가] 이전 씬의 이름을 저장할 변수
    public string PreviousSceneName { get; private set; } = "";

    // ... (UI 변수 및 Awake는 기존과 동일) ...
    [Header("UI Components")]
    [SerializeField] private GameObject loadingCanvasPrefab;
    private GameObject loadingCanvasInstance;
    private Slider progressBar;
    private TextMeshProUGUI loadingText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        // [핵심 추가] 로딩을 시작하기 전에, 현재 씬(떠나는 씬)의 이름을 기록해둡니다.
        PreviousSceneName = SceneManager.GetActiveScene().name;
        
        StartCoroutine(LoadSequence(sceneName));
    }

    private IEnumerator LoadSequence(string sceneName)
    {
        // ... (이전과 동일한 로딩 시퀀스 로직) ...
        // 1. 로딩 UI 생성
        if (loadingCanvasInstance == null)
        {
            loadingCanvasInstance = Instantiate(loadingCanvasPrefab);
            DontDestroyOnLoad(loadingCanvasInstance);
            progressBar = loadingCanvasInstance.GetComponentInChildren<Slider>();
            loadingText = loadingCanvasInstance.GetComponentInChildren<TextMeshProUGUI>();
        }
        loadingCanvasInstance.SetActive(true);
        progressBar.value = 0f;

        // 2. 데이터 매니저 초기화 (공통)
        loadingText.text = "데이터 매니저 초기화 중...";

        progressBar.value = 0.3f;

        // 3. 씬 로드
        loadingText.text = "이동 중...";
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            progressBar.value = 0.3f + (progress * 0.4f);
            if (op.progress >= 0.9f) op.allowSceneActivation = true;
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        yield return null;

        // 4. 씬별 초기화 실행
        loadingText.text = "환경 설정 적용 중...";
        progressBar.value = 0.8f;

        var initializers = FindObjectsOfType<MonoBehaviour>().OfType<ISceneInitializer>();
        foreach (var initializer in initializers)
        {
            yield return StartCoroutine(initializer.Initialize());
        }

        progressBar.value = 1.0f;
        loadingText.text = "완료!";
        yield return new WaitForSeconds(0.5f);
        loadingCanvasInstance.SetActive(false);
    }
}