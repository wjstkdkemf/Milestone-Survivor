using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(CanvasGroup))]
public class PanelMessage : MonoBehaviour
{
    // 1단계에서 만든 자식 Text 오브젝트를 연결할 필드
    [SerializeField]
    private TMP_Text messageText;
    [SerializeField]
    private float fadeDuration = 0.5f; // 페이드인/아웃에 걸리는 시간
    private CanvasGroup canvasGroup;

    // 이전에 실행 중이던 코루틴을 저장할 변수
    private Coroutine activeCoroutine;

    /// <summary>
    /// 이 스크립트는 패널이 비활성화된 상태로 시작해야 하므로,
    /// 씬에서 미리 꺼두는 것을 권장합니다.
    /// </summary>
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    private void Start()
    {
        // 시작 시 알파값을 0으로 설정해 완전히 투명하게 만듦
        canvasGroup.alpha = 0f;
        // 패널이 투명할 때는 클릭 등 상호작용이 되지 않도록 설정
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 메시지를 설정하고 패널을 활성화한 뒤, 일정 시간 후 끄는 코루틴을 시작합니다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="duration">패널이 표시될 시간 (초)</param>
    public void ShowMessage(string message, float duration)
    {
// 1. 혹시 이전에 실행 중이던 코루틴이 있다면 즉시 중지
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        // 2. 텍스트 설정 (알파값이 0이라 아직 보이진 않음)
        messageText.text = message;

        // 3. 페이드인 -> 대기 -> 페이드아웃 코루틴 시작
        activeCoroutine = StartCoroutine(C_FadeInThenOut(duration));
    }

    /// <summary>
    /// N초간 기다린 뒤 패널을 끄는 코루틴
    /// </summary>
private IEnumerator C_FadeInThenOut(float duration)
    {
        // --- 1. 페이드인 (Fade-in) ---
        // 패널이 상호작용 가능하도록 설정
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // Lerp(시작값, 끝값, 진행률)
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration); // 0 -> 1
            yield return null;
        }
        canvasGroup.alpha = 1f; // 확실하게 1로

        // --- 2. 메시지 유지 ---
        yield return new WaitForSeconds(duration);

        // --- 3. 페이드아웃 (Fade-out) ---
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); // 1 -> 0
            yield return null;
        }
        canvasGroup.alpha = 0f; // 확실하게 0으로
        
        // 패널이 다시 상호작용 불가능하도록 설정
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // 4. 코루틴 종료
        activeCoroutine = null;
    }
}
