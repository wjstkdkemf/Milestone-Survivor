using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponVisualController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform weaponVisual;

    public WeaponVisualMode mode;
    [SerializeField] private Vector2 directionalOffset = new Vector2(0.6f, 0.6f);
    [SerializeField] private float spriteRotationOffset = 0f;
    public float orbitRadius = 0.7f;
    public float orbitSpeed = 120f;

    private Vector3 baseLocalPosition;
    private Transform target;
    private Vector2 aimDirection = Vector2.right;


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetBaseLocalPosition(Vector3 position)
    {
        baseLocalPosition = position;
    }
    public void SetDirectionalOffset(Vector2 offset)
    {
        directionalOffset = offset;
    }
    public void SetRotationOffset(float offset)
    {
        spriteRotationOffset = offset;
    }

    private void LateUpdate()
    {
        switch (mode)
        {
            case WeaponVisualMode.OrbitAroundPlayer:
                UpdateOrbit();
                break;

            case WeaponVisualMode.FacePlayerDirection:
                UpdateFaceDirection();
                break;

            case WeaponVisualMode.FaceTarget:
                UpdateFaceTarget();
                break;
        }
    }

    private void UpdateOrbit()
    {
        float angle = Time.time * orbitSpeed * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * orbitRadius;

        weaponVisual.localPosition = baseLocalPosition + offset;
        //weaponVisual.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);//진짜 빙글거리는건 스킬에서 처리할 예정. 물론 추후 수정가능성 있음.
    }

    /*private void UpdateFaceDirection()
    {
        Vector3 pos = rightFacingLocalPosition;
        pos.x *= facingSign;

        weaponVisual.localPosition = pos;
        weaponVisual.localRotation = facingSign > 0
            ? Quaternion.identity
            : Quaternion.Euler(0f, 0f, 180f);
    }*/
    private void UpdateFaceDirection()
    {
        ApplyFacingPose(aimDirection);
    }

    private void UpdateFaceTarget()
    {
        if (target == null || player == null)
        {
            UpdateFaceDirection();
            return;
        }

        Vector2 dir = target.position - player.position;
        ApplyFacingPose(dir);
    }
    private void ApplyFacingPose(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f)
            dir = Vector2.right;

        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Vector3 pos = baseLocalPosition;
        pos.x += dir.x * directionalOffset.x;
        pos.y += dir.y * directionalOffset.y;

        weaponVisual.localPosition = pos;
        weaponVisual.localRotation = Quaternion.Euler(0f, 0f, angle + spriteRotationOffset);
    }

    public void SetAimDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        aimDirection = direction;
    }
}