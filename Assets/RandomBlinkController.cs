using System.Collections;
using UnityEngine;

/// <summary>
/// 현재 Animator Controller의 공통 Blink Trigger를
/// 불규칙한 간격으로 실행합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class RandomBlinkController : MonoBehaviour
{
    private static readonly int BlinkHash = Animator.StringToHash("Blink");

    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Timing")]
    [SerializeField] private Vector2 blinkInterval = new(2.5f, 5f);

    [Tooltip("일정 확률로 짧은 시간 뒤 한 번 더 깜빡입니다.")]
    [SerializeField, Range(0f, 1f)]
    private float doubleBlinkChance = 0.1f;

    [SerializeField, Min(0.05f)]
    private float doubleBlinkDelay = 0.15f;

    [Header("State Rule")]
    [Tooltip("Animator에서 Idle 상태에 지정한 Tag 이름")]
    [SerializeField] private string idleStateTag = "Idle";

    private Coroutine blinkRoutine;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        blinkRoutine = StartCoroutine(BlinkLoop());
    }

    private void OnDisable()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (animator != null)
        {
            animator.ResetTrigger(BlinkHash);
        }
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            float min = Mathf.Min(blinkInterval.x, blinkInterval.y);
            float max = Mathf.Max(blinkInterval.x, blinkInterval.y);

            yield return new WaitForSeconds(Random.Range(min, max));

            if (!CanBlink())
            {
                continue;
            }

            animator.SetTrigger(BlinkHash);
            
            if (Random.value <= doubleBlinkChance)
            {
                yield return new WaitForSeconds(doubleBlinkDelay);

                if (CanBlink())
                {
                    animator.SetTrigger(BlinkHash);
                }
            }
        }
    }

    private bool CanBlink()
    {
        if (animator == null || !animator.isActiveAndEnabled)
        {
            return false;
        }

        if (animator.IsInTransition(0))
        {
            return false;
        }

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.IsTag(idleStateTag);
    }

    /// <summary>
    /// 캐릭터별 깜빡임 성향이 필요할 때 설정합니다.
    /// </summary>
    public void SetBlinkInterval(float minSeconds, float maxSeconds)
    {
        blinkInterval = new Vector2(
            Mathf.Max(0.1f, minSeconds),
            Mathf.Max(0.1f, maxSeconds));
    }
}