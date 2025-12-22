using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    // 현재 활성화된 무기들 목록 (최대 6개 제한 가능)
    public List<WeaponBase> activeWeapons = new List<WeaponBase>();
    [SerializeField] private Transform weaponHolder;

    // 레벨업해서 무기를 골랐을 때 호출!
    public void AddWeapon(WeaponDataSO data)
    {
        // 1. 해당 무기의 프리팹을 플레이어 자식으로 생성
        GameObject newWeaponObj = Instantiate(data.weaponPrefab, weaponHolder);
        newWeaponObj.transform.localPosition = Vector3.zero;
        newWeaponObj.transform.localRotation = Quaternion.identity;
        
        // 2. 스크립트 가져오기
        WeaponBase newWeapon = newWeaponObj.GetComponent<WeaponBase>();
        
        // 3. 초기화 및 리스트 추가
        newWeapon.Initialize(data);
        newWeapon.myData = data;
        activeWeapons.Add(newWeapon);
    }

    void Update()
    {
        // 내가 가진 무기들만 동작시킴
        foreach (var weapon in activeWeapons)
        {
            if (weapon != null && weapon.gameObject.activeInHierarchy)
                weapon.OnUpdate();
        }
    }
    public void ToggleCombatMode(bool isCombat)
    {
        // 내가 가진 모든 무기들을 순회하면서
        foreach (var weapon in activeWeapons)
        {
            if (weapon != null)
            {
                // 무기 오브젝트 자체를 껐다 킵니다.
                // (꺼지면 Update가 안 돌아가니 공격도 멈추고, 화면에서도 사라집니다)
                weapon.gameObject.SetActive(isCombat);
            }
        }
    }
}
