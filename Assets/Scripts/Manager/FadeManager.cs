using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System; // Action을 사용하기 위해 필요

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }
    public Image fadeImage; // 화면 전체를 덮는 검은색 UI Image
    public float fadeDuration = 1.0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 화면 어둡게 (완료되면 onComplete 액션 실행)
    public void FadeOut(Action onComplete)
    {
        StartCoroutine(FadeOutRoutine(onComplete));
    }

    // 화면 밝게
    public void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeOutRoutine(Action onComplete)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, timer / fadeDuration);
            yield return null;
        }
        fadeImage.color = Color.black; // 확실하게 검은색으로

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<Teleporter>().isTeleporting = false;
        
        onComplete?.Invoke(); // 페이드아웃이 끝났으니, MapManager의 다음 작업을 실행
    }

    private IEnumerator FadeInRoutine()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - (timer / fadeDuration));
            yield return null;
        }
        fadeImage.color = Color.clear; // 확실하게 투명하게
    }

    // 화면을 즉시 검은색으로 설정
    public void SetBlack()
    {
        fadeImage.color = Color.black;
    }
}