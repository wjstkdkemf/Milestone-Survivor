using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponDataSO myData;
    // 무기 데이터(SO)를 받아서 초기화하는 함수
    public abstract void Initialize(WeaponDataSO data);

    // 플레이어의 Update에서 매 프레임 호출해줄 함수
    public abstract void OnUpdate();
    public virtual void LevelUp()
    {
        // 자식들이 오버라이드해서 구현
        // 예: OrbWeapon은 orbCount++, AxeWeapon은 데미지++
        Debug.Log("무기 레벨업!");
    }
}