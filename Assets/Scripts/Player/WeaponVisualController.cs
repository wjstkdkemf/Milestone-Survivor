using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponVisualController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform weaponVisual;

    public WeaponVisualMode mode;
    public Vector3 rightFacingLocalPosition = new Vector3(0.6f, -0.2f, 0f);
    public float orbitRadius = 0.7f;
    public float orbitSpeed = 120f;

    private int facingSign = 1;
    private Transform target;
    private Vector2 aimDirection = Vector2.right;

    public void SetFacing(int sign)
    {
        facingSign = sign >= 0 ? 1 : -1;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
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

        weaponVisual.localPosition = offset;
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
        Vector2 dir = aimDirection.normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        weaponVisual.localPosition = rotation * rightFacingLocalPosition;
        weaponVisual.localRotation = rotation;
    }

    private void UpdateFaceTarget()
    {
        if (target == null)
        {
            UpdateFaceDirection();
            return;
        }

        Vector2 dir = target.position - player.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        weaponVisual.localPosition = dir.normalized * rightFacingLocalPosition.magnitude;
        weaponVisual.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
    public void SetAimDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        aimDirection = direction;
    }
}