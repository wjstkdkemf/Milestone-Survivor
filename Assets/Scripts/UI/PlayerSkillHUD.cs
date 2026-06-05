using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerSkillHUD : MonoBehaviour
{
    [Header("UI 연결")]
    public Transform iconContainer;
    public GameObject iconPrefab; 

    [Header("플레이어 연결")]
    public PlayerWeaponController weaponController;

    private List<GameObject> spawnedIcons = new List<GameObject>();

    public void RefreshWeaponIcons()
    {
        foreach (GameObject iconObj in spawnedIcons)
        {
            if (iconObj != null)
            {
                Destroy(iconObj);
            }
        }
        spawnedIcons.Clear(); // 리스트 비우기

        foreach (WeaponBase weapon in weaponController.activeWeapons)
        {
            if (weapon == null || weapon.myData == null || weapon.myData.upgradeData == null) 
                continue;

            GameObject newIconObj = Instantiate(iconPrefab, iconContainer);

            WeaponIconUI iconUI = newIconObj.GetComponent<WeaponIconUI>();

            if (iconUI != null)
            {
                iconUI.SetIcon(weapon.myData.upgradeData.Icon);
            }

            spawnedIcons.Add(newIconObj);
        }

        foreach (WeaponBase weapon in weaponController.WeaponsFromEquipment)
        {
            if (weapon == null || weapon.myData == null || weapon.myData.upgradeData == null) 
                continue;

            GameObject newIconObj = Instantiate(iconPrefab, iconContainer);
            
            WeaponIconUI iconUI = newIconObj.GetComponent<WeaponIconUI>();

            if (iconUI != null)
            {
                iconUI.SetIcon(weapon.myData.upgradeData.Icon);
            }

            spawnedIcons.Add(newIconObj);
        }
    }
}
