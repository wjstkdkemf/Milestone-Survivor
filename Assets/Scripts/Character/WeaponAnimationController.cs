using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimationController : MonoBehaviour
{
    [SerializeField] private Transform motionPivot;

    private Coroutine motionCoroutine;
    private int currentPriority = int.MinValue;
    private WeaponMotionData weaponMotionData;

    private float sameMotionIgnoreThreshold = 0.6f;
    private float currentNormalizedTime = 1.0f;
    private static readonly Vector3 IdlePosition = Vector3.zero;
    private static readonly Quaternion IdleRotation = Quaternion.identity;
    private static readonly Vector3 IdleScale = Vector3.one;

    public void PlayMotion(WeaponMotionData motion)
    {
        if (weaponMotionData == motion && currentNormalizedTime < sameMotionIgnoreThreshold) return; // 너무 이른 재발동은 시각 모션 생략
        if (motion == null || motionPivot == null) return;
        if (!CanPlay(motion))
            return;

        if (motionCoroutine != null)
            StopCoroutine(motionCoroutine);

        weaponMotionData = motion;
        currentNormalizedTime = 0f;
        currentPriority = motion.priority;
        motionCoroutine = StartCoroutine(PlayMotionRoutine(motion));
    }

    private IEnumerator PlayMotionRoutine(WeaponMotionData motion)
    {
        Vector3 startPos = motionPivot.localPosition;
        Quaternion startRot = motionPivot.localRotation;
        Vector3 startScale = motionPivot.localScale;

        Vector3 motionStartPos = EvaluatePosition(motion, 0f);
        Quaternion motionStartRot = EvaluateRotation(motion, 0f);
        Vector3 motionStartScale = EvaluateScale(motion, 0f);

        float blendTime = Mathf.Max(0.001f, motion.blendInTime);
        float blendTimer = 0f;

        while (blendTimer < blendTime)
        {
            blendTimer += Time.deltaTime;
            float t = Mathf.Clamp01(blendTimer / blendTime);

            motionPivot.localPosition = Vector3.Lerp(startPos, motionStartPos, t);
            motionPivot.localRotation = Quaternion.Slerp(startRot, motionStartRot, t);
            motionPivot.localScale = Vector3.Lerp(startScale, motionStartScale, t);

            yield return null;
        }

        float timer = 0f;
        float duration = Mathf.Max(0.001f, motion.duration);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(timer / duration);
            currentNormalizedTime = normalizedTime;

            motionPivot.localPosition = EvaluatePosition(motion, normalizedTime);
            motionPivot.localRotation = EvaluateRotation(motion, normalizedTime);
            motionPivot.localScale = EvaluateScale(motion, normalizedTime);

            yield return null;
        }
        weaponMotionData = null;
        motionCoroutine = StartCoroutine(ReturnToIdleRoutine(motion.blendOutTime));
    }

    private IEnumerator ReturnToIdleRoutine(float duration)
    {
        Vector3 startPos = motionPivot.localPosition;
        Quaternion startRot = motionPivot.localRotation;
        Vector3 startScale = motionPivot.localScale;

        float timer = 0f;
        duration = Mathf.Max(0.001f, duration);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            float eased = EaseOutCubic(t);

            motionPivot.localPosition = Vector3.Lerp(startPos, IdlePosition, eased);
            motionPivot.localRotation = Quaternion.Slerp(startRot, IdleRotation, eased);
            motionPivot.localScale = Vector3.Lerp(startScale, IdleScale, eased);

            yield return null;
        }

        motionPivot.localPosition = IdlePosition;
        motionPivot.localRotation = IdleRotation;
        motionPivot.localScale = IdleScale;
        
        currentNormalizedTime = 1f;
        motionCoroutine = null;
        currentPriority = int.MinValue;
    }
    private bool CanPlay(WeaponMotionData next)
    {
        if (motionCoroutine == null) return true;
        if (next.priority > currentPriority) return true;
        if (next.priority == currentPriority && next.canInterrupt) return true;
        return false;
    }
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private Vector3 EvaluatePosition(WeaponMotionData motion, float t)
    {
        return new Vector3(
            EvaluateCurve(motion.positionX, t, 0f),
            EvaluateCurve(motion.positionY, t, 0f),
            0f
        );
    }

    private Quaternion EvaluateRotation(WeaponMotionData motion, float t)
    {
        float z = EvaluateCurve(motion.rotationZ, t, 0f);
        z *= motion.rotationMultiplier;
        return Quaternion.Euler(0f, 0f, z);
    }

    private Vector3 EvaluateScale(WeaponMotionData motion, float t)
    {
        float scale = EvaluateCurve(motion.scale, t, 1f);
        return new Vector3(scale, scale, 1f);
    }
    private float EvaluateCurve(AnimationCurve curve, float t, float fallback)
    {
        return curve != null && curve.length > 0
            ? curve.Evaluate(t)
            : fallback;
    }
}