using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    // 현재 활성화된 무기들 목록 (최대 6개 제한 가능)
    public List<WeaponBase> activeWeapons = new List<WeaponBase>();
    public List<WeaponBase> WeaponsFromEquipment = new List<WeaponBase>();

    [SerializeField] private Transform weaponHolder;
    public PlayerSkillHUD playerHUD;

    // 레벨업해서 무기를 골랐을 때 호출!
    public void AddWeapon(WeaponDataSO data)
    {
        if (data.fusionWeaponData != null)
            CheckFusion(data);

        if(!CheckLevelUp(data))
        {
            // 해당 무기의 프리팹을 플레이어 자식으로 생성
            GameObject newWeaponObj = Instantiate(data.weaponPrefab, weaponHolder);
            newWeaponObj.transform.localPosition = Vector3.zero;
            newWeaponObj.transform.localRotation = Quaternion.identity;
            
            // 스크립트 가져오기
            WeaponBase newWeapon = newWeaponObj.GetComponent<WeaponBase>();
            
            // 초기화 및 리스트 추가
            newWeapon.Initialize(data);
            newWeapon.myData = data;
            activeWeapons.Add(newWeapon);

            if (playerHUD != null)
            {
                playerHUD.RefreshWeaponIcons();
            }
        }
    }
    public void AddWeaponFromEquipment(WeaponDataSO data)
    {
        if (data.fusionWeaponData != null)
            CheckFusion(data);

        if(!CheckLevelUp(data))
        {
            // 해당 무기의 프리팹을 플레이어 자식으로 생성
            GameObject newWeaponObj = Instantiate(data.weaponPrefab, weaponHolder);
            newWeaponObj.transform.localPosition = Vector3.zero;
            newWeaponObj.transform.localRotation = Quaternion.identity;
            
            // 스크립트 가져오기
            WeaponBase newWeapon = newWeaponObj.GetComponent<WeaponBase>();
            
            // 초기화 및 리스트 추가
            newWeapon.Initialize(data);
            newWeapon.myData = data;
            WeaponsFromEquipment.Add(newWeapon);

            if (playerHUD != null)
            {
                playerHUD.RefreshWeaponIcons();
            }
        }
    }
    public void RemoveWeaponFromEquipment(WeaponDataSO data)
    {
        for (int i = WeaponsFromEquipment.Count - 1; i >= 0; i--)
        {
            // 데이터(SO)가 일치하는지 확인
            if (WeaponsFromEquipment[i].myData == data)
            {
                WeaponBase weaponToRemove = WeaponsFromEquipment[i];

                // 리스트에서 제거 (장부 정리)
                WeaponsFromEquipment.RemoveAt(i);

                // 실제 게임 오브젝트 파괴 (화면에서 제거)
                if (weaponToRemove != null)
                {
                    Destroy(weaponToRemove.gameObject);
                }

                if (playerHUD != null)
                {
                    playerHUD.RefreshWeaponIcons();
                }

                return;
            }
        }
    }
    public void RemoveWeapon(WeaponDataSO data)
    {
        for (int i = activeWeapons.Count - 1; i >= 0; i--)
        {
            if (activeWeapons[i].myData == data)
            {
                WeaponBase weaponToRemove = activeWeapons[i];

                // 리스트에서 제거
                activeWeapons.RemoveAt(i);

                // 실제 오브젝트 파괴
                if (weaponToRemove != null)
                {
                    Destroy(weaponToRemove.gameObject);
                }

                if (playerHUD != null)
                {
                    playerHUD.RefreshWeaponIcons();
                }

                return;
            }
        }
    }
    public void CheckFusion(WeaponDataSO data)
    {
        foreach (WeaponDataSO weapon in data.fusionWeaponData)
        {
            RemoveWeapon(weapon);
        }
    }
    public bool CheckLevelUp(WeaponDataSO data)
    {
        foreach (var weapon in activeWeapons)
        {
            if(weapon.myData == data)
            {
                weapon.LevelUp();
                return true;
            }
        }
        return false;
    }

    void LateUpdate()
    {
        // 내가 가진 무기들만 동작시킴
        foreach (var weapon in activeWeapons)
        {
            if (weapon != null && weapon.gameObject.activeInHierarchy)
                weapon.OnUpdate();
        }
        foreach (var weapon in WeaponsFromEquipment)
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

        foreach (var weapon in WeaponsFromEquipment)
        {
            if (weapon != null)
            {
                weapon.gameObject.SetActive(isCombat);
            }
        }
    }

    public void ClearEquipmentSkills()
    {
        for (int i = WeaponsFromEquipment.Count - 1; i >= 0; i--)
        {
            WeaponBase weaponToRemove = WeaponsFromEquipment[i];
            // 실제 게임 오브젝트 파괴 (화면에서 제거)
            if (weaponToRemove != null)
            {
                Destroy(weaponToRemove.gameObject);
            }
        }
        
        if (playerHUD != null)
        {
            playerHUD.RefreshWeaponIcons();
        }
        WeaponsFromEquipment.Clear();
    }
}
