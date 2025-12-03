using UnityEngine;

[CreateAssetMenu(fileName = "New Orb Data", menuName = "Weapon Data/Orb Weapon")]
public class OrbWeaponDataSO : WeaponDataSO // 기본 무기 데이터를 상속받음!
{
    [Header("Orb Specific Stats")]
    public int orbCount = 1;      // 구체 개수
    public float radius = 2f;     // 회전 반경
    public float rotationSpeed = 50f; // 회전 속도
    public GameObject orbProjectilePrefab; // 실제 돌아가는 작은 구체 프리팹 (IceOrb 등)
}