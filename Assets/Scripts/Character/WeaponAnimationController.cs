using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimationController : MonoBehaviour
{
    [SerializeField] private Transform weaponVisual;

    private Coroutine motionCoroutine;

    private Vector3 idleLocalPosition;
    private Quaternion idleLocalRotation;
    private Vector3 idleLocalScale;

    private void Awake()
    {
        idleLocalPosition = weaponVisual.localPosition;
        idleLocalRotation = weaponVisual.localRotation;
        idleLocalScale = weaponVisual.localScale;
    }

    public void PlayMotion(WeaponMotionData motion, Vector2 direction)
    {
        if (motionCoroutine != null)
            StopCoroutine(motionCoroutine);

        motionCoroutine = StartCoroutine(PlayMotionRoutine(motion, direction));
    }

    private IEnumerator PlayMotionRoutine(WeaponMotionData motion, Vector2 direction)
    {
        Vector3 startPos = weaponVisual.localPosition;
        Quaternion startRot = weaponVisual.localRotation;
        Vector3 startScale = weaponVisual.localScale;

        Vector3 motionStartPos = EvaluatePosition(motion, 0f);
        Quaternion motionStartRot = EvaluateRotation(motion, 0f);
        Vector3 motionStartScale = EvaluateScale(motion, 0f);

        float blendTime = motion.blendInTime;
        float blendTimer = 0f;

        while (blendTimer < blendTime)
        {
            blendTimer += Time.deltaTime;
            float t = Mathf.Clamp01(blendTimer / blendTime);

            weaponVisual.localPosition = Vector3.Lerp(startPos, motionStartPos, t);
            weaponVisual.localRotation = Quaternion.Slerp(startRot, motionStartRot, t);
            weaponVisual.localScale = Vector3.Lerp(startScale, motionStartScale, t);

            yield return null;
        }

        float timer = 0f;

        while (timer < motion.duration)
        {
            timer += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(timer / motion.duration);

            weaponVisual.localPosition = EvaluatePosition(motion, normalizedTime);
            weaponVisual.localRotation = EvaluateRotation(motion, normalizedTime);
            weaponVisual.localScale = EvaluateScale(motion, normalizedTime);

            yield return null;
        }

        motionCoroutine = StartCoroutine(ReturnToIdleRoutine(0.1f));
    }

    private IEnumerator ReturnToIdleRoutine(float duration)
    {
        Vector3 startPos = weaponVisual.localPosition;
        Quaternion startRot = weaponVisual.localRotation;
        Vector3 startScale = weaponVisual.localScale;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            weaponVisual.localPosition = Vector3.Lerp(startPos, idleLocalPosition, t);
            weaponVisual.localRotation = Quaternion.Slerp(startRot, idleLocalRotation, t);
            weaponVisual.localScale = Vector3.Lerp(startScale, idleLocalScale, t);

            yield return null;
        }

        weaponVisual.localPosition = idleLocalPosition;
        weaponVisual.localRotation = idleLocalRotation;
        weaponVisual.localScale = idleLocalScale;
    }

    private Vector3 EvaluatePosition(WeaponMotionData motion, float t)
    {
        return new Vector3(
            motion.positionX.Evaluate(t),
            motion.positionY.Evaluate(t),
            0f
        );
    }

    private Quaternion EvaluateRotation(WeaponMotionData motion, float t)
    {
        float z = motion.rotationZ.Evaluate(t);
        return Quaternion.Euler(0f, 0f, z);
    }

    private Vector3 EvaluateScale(WeaponMotionData motion, float t)
    {
        float scale = motion.scale.Evaluate(t);
        return new Vector3(scale, scale, 1f);
    }
}