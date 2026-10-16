using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUIEffectManager : MonoBehaviour
{
    [SerializeField] private RectTransform itemIconRect;
    [SerializeField] private UIFrameEffect enhanceTryEffect;
    [SerializeField] private UIFrameEffect enhanceSuccessEffect;
    [SerializeField] private UIFrameEffect enhanceFailureEffect;

    private RectTransform animatingIcon;
    private Vector3 originalScale;
    private Coroutine punchRoutine;
    public void OnEnhanceTry()
    {
        enhanceTryEffect.Play();
    }
    public void OnEnhanceSuccess()
    {
        if (!isActiveAndEnabled)
            return;

        enhanceSuccessEffect?.Play();
        StopAndRestoreIcon();

        if (itemIconRect == null ||
            !itemIconRect.gameObject.activeInHierarchy)
            return;
    
        animatingIcon = itemIconRect;
        originalScale = itemIconRect.localScale;
        punchRoutine = StartCoroutine(PunchIcon(itemIconRect));
    }
    public void OnEnhanceFailure()
    {
        enhanceFailureEffect.Play();
    }
    
    public IEnumerator PunchIcon(RectTransform icon)
    {
        float duration = 0.18f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;

            float scale = t < 0.5f
                ? Mathf.Lerp(1f, 1.12f, t / 0.5f)
                : Mathf.Lerp(1.12f, 1f, (t - 0.5f) / 0.5f);

            icon.localScale = originalScale * scale;
            yield return null;
        }

        icon.localScale = originalScale;
        animatingIcon = null;
        punchRoutine = null;    
    }
    private void StopAndRestoreIcon()
    {
        if (punchRoutine != null)
        {
            StopCoroutine(punchRoutine);
            punchRoutine = null;
        }

        if (animatingIcon != null)
        {
            animatingIcon.localScale = originalScale;
            animatingIcon = null;
        }
    }

    private void OnDisable()
    {
        StopAndRestoreIcon();
    }
}
