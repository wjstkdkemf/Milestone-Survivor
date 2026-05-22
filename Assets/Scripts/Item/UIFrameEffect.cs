using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFrameEffect : MonoBehaviour
{
    [SerializeField] private Image effectImage;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameInterval = 0.06f;
    private Coroutine playRoutine;

    private void Awake()
    {
        if (effectImage != null)
        {
            effectImage.enabled = false;
            effectImage.sprite = null;
            effectImage.raycastTarget = false;
        }
    }

    public void Play()
    {
        if (effectImage == null || frames == null || frames.Length == 0)
            return;

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        effectImage.enabled = true;
        effectImage.color = Color.white;

        for (int i = 0; i < frames.Length; i++)
        {
            effectImage.sprite = frames[i];
            yield return new WaitForSecondsRealtime(frameInterval);
        }

        effectImage.sprite = null;
        effectImage.enabled = false;

        playRoutine = null;
    }
}