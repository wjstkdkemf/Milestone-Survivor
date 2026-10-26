using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    public List<WeaponBase> activeWeapons = new List<WeaponBase>();
    public List<WeaponBase> WeaponsFromEquipment = new List<WeaponBase>();
    [SerializeField] private Transform weaponHolder;
    public PlayerSkillHUD playerHUD;

    private sealed class WeaponState
    {
        public int LearnedLevel;
        public int EquipmentLevel;
        public WeaponBase Weapon;
    }

    private readonly Dictionary<WeaponDataSO, WeaponState> weaponStates = new Dictionary<WeaponDataSO, WeaponState>();
    private readonly Dictionary<WeaponDataSO, int> requestedEquipmentLevels = new Dictionary<WeaponDataSO, int>();
    private readonly List<WeaponDataSO> stateKeys = new List<WeaponDataSO>();
    private bool combatEnabled = true;

    public void AddWeapon(WeaponDataSO data)
    {
        if (data == null) return;
        CheckFusion(data);
        WeaponState state = GetOrCreateState(data);
        state.LearnedLevel++;
        SyncWeapon(data, state);
        RefreshHUD();
    }

    public void AddWeaponFromEquipment(WeaponDataSO data)
    {
        if (data == null) return;
        // Borrowed equipment skills must not consume learned fusion ingredients.
        WeaponState state = GetOrCreateState(data);
        state.EquipmentLevel++;
        SyncWeapon(data, state);
        RefreshHUD();
    }

    public void RemoveWeaponFromEquipment(WeaponDataSO data)
    {
        if (data == null || !weaponStates.TryGetValue(data, out WeaponState state) || state.EquipmentLevel == 0)
            return;

        state.EquipmentLevel--;
        SyncWeapon(data, state);
        RefreshHUD();
    }

    public void RemoveWeapon(WeaponDataSO data)
    {
        if (data == null || !weaponStates.TryGetValue(data, out WeaponState state)) return;
        state.LearnedLevel = 0;
        SyncWeapon(data, state);
        RefreshHUD();
    }

    public void CheckFusion(WeaponDataSO data)
    {
        if (data == null || data.fusionWeaponData == null) return;
        foreach (WeaponDataSO ingredient in data.fusionWeaponData)
            RemoveWeapon(ingredient);
    }

    public bool CheckLevelUp(WeaponDataSO data)
    {
        if (data == null || !weaponStates.TryGetValue(data, out WeaponState state) || state.LearnedLevel == 0)
            return false;

        state.LearnedLevel++;
        SyncWeapon(data, state);
        RefreshHUD();
        return true;
    }

    private void LateUpdate()
    {
        if (Time.timeScale <= 0f)
            return;


        foreach (WeaponBase weapon in activeWeapons)
            if (weapon != null && weapon.gameObject.activeInHierarchy) weapon.OnUpdate();
        foreach (WeaponBase weapon in WeaponsFromEquipment)
            if (weapon != null && weapon.gameObject.activeInHierarchy) weapon.OnUpdate();
    }

    public void ToggleCombatMode(bool isCombat)
    {
        combatEnabled = isCombat;
        foreach (WeaponBase weapon in activeWeapons)
            if (weapon != null) weapon.gameObject.SetActive(isCombat);
        foreach (WeaponBase weapon in WeaponsFromEquipment)
            if (weapon != null) weapon.gameObject.SetActive(isCombat);
    }

    public void ClearEquipmentSkills()
    {
        SetEquipmentSkills(null);
    }

    public int GetLearnedLevel(WeaponDataSO data)
    {
        return data != null && weaponStates.TryGetValue(data, out WeaponState state) ? state.LearnedLevel : 0;
    }

    public int GetEquipmentLevel(WeaponDataSO data)
    {
        return data != null && weaponStates.TryGetValue(data, out WeaponState state) ? state.EquipmentLevel : 0;
    }

    public void SetEquipmentSkills(IReadOnlyList<WeaponDataSO> skills)
    {
        requestedEquipmentLevels.Clear();
        if (skills != null)
        {
            for (int i = 0; i < skills.Count; i++)
            {
                WeaponDataSO data = skills[i];
                if (data == null) continue;
                requestedEquipmentLevels.TryGetValue(data, out int count);
                requestedEquipmentLevels[data] = count + 1;
                GetOrCreateState(data);
            }
        }

        // Snapshot keys because removing the last level also removes its state.
        stateKeys.Clear();
        stateKeys.AddRange(weaponStates.Keys);
        bool changed = false;
        foreach (WeaponDataSO data in stateKeys)
        {
            WeaponState state = weaponStates[data];
            requestedEquipmentLevels.TryGetValue(data, out int level);
            if (state.EquipmentLevel == level && state.Weapon != null) continue;
            state.EquipmentLevel = level;
            SyncWeapon(data, state);
            changed = true;
        }
        if (changed) RefreshHUD();
    }

    private WeaponState GetOrCreateState(WeaponDataSO data)
    {
        if (!weaponStates.TryGetValue(data, out WeaponState state))
        {
            state = new WeaponState();
            weaponStates.Add(data, state);
        }
        return state;
    }

    private void SyncWeapon(WeaponDataSO data, WeaponState state)
    {
        int totalLevel = state.LearnedLevel + state.EquipmentLevel;
        WeaponBase weapon = state.Weapon;
        if (totalLevel == 0)
        {
            activeWeapons.Remove(weapon);
            WeaponsFromEquipment.Remove(weapon);
            if (weapon != null)
            {
                weapon.gameObject.SetActive(false);
                Destroy(weapon.gameObject);
            }
            weaponStates.Remove(data);
            return;
        }

        if (weapon == null)
        {
            if (data.weaponPrefab == null || data.weaponPrefab.GetComponent<WeaponBase>() == null)
            {
                Debug.LogError($"[PlayerWeaponController] Missing weapon prefab/component: {data.name}");
                return;
            }
            GameObject instance = Instantiate(data.weaponPrefab, weaponHolder);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            weapon = instance.GetComponent<WeaponBase>();
            state.Weapon = weapon;
            weapon.myData = data;
            weapon.CurrentLevel = 1;
            weapon.Initialize(data);
            instance.SetActive(combatEnabled);
        }

        weapon.SetLevel(totalLevel);
        List<WeaponBase> target = state.LearnedLevel > 0 ? activeWeapons : WeaponsFromEquipment;
        List<WeaponBase> other = state.LearnedLevel > 0 ? WeaponsFromEquipment : activeWeapons;
        other.Remove(weapon);
        if (!target.Contains(weapon)) target.Add(weapon);
    }

    private void RefreshHUD()
    {
        if (playerHUD != null) playerHUD.RefreshWeaponIcons();
    }

    public WeaponBase GetWeaponById(string weaponId)
    {
        TryGetWeaponById(weaponId, out WeaponBase weapon);
        return weapon;
    }

    public WeaponBase GetWeapon(WeaponDataSO data)
    {
        TryGetWeapon(data, out WeaponBase weapon);
        return weapon;
    }

    public bool TryGetWeapon(WeaponDataSO data, out WeaponBase weapon, bool includeEquipmentWeapons = false)
    {
        weapon = null;

        if (data == null)
            return false;

        for (int i = activeWeapons.Count - 1; i >= 0; i--)
        {
            if (activeWeapons[i] != null && activeWeapons[i].myData == data)
            {
                weapon = activeWeapons[i];
                return true;
            }
        }

        if (includeEquipmentWeapons)
        {
            for (int i = WeaponsFromEquipment.Count - 1; i >= 0; i--)
            {
                if (WeaponsFromEquipment[i] != null && WeaponsFromEquipment[i].myData == data)
                {
                    weapon = WeaponsFromEquipment[i];
                    return true;
                }
            }
        }

        if (!string.IsNullOrEmpty(data.WeaponId))
            return TryGetWeaponById(data.WeaponId, out weapon, includeEquipmentWeapons);

        return false;
    }

    public bool TryGetWeaponById(string weaponId, out WeaponBase weapon, bool includeEquipmentWeapons = false)
    {
        weapon = null;

        if (string.IsNullOrEmpty(weaponId))
            return false;

        for (int i = activeWeapons.Count - 1; i >= 0; i--)
        {
            if (activeWeapons[i] != null && activeWeapons[i].WeaponID == weaponId)
            {
                weapon = activeWeapons[i];
                return true;
            }
        }

        if (includeEquipmentWeapons)
        {
            for (int i = WeaponsFromEquipment.Count - 1; i >= 0; i--)
            {
                if (WeaponsFromEquipment[i] != null && WeaponsFromEquipment[i].WeaponID == weaponId)
                {
                    weapon = WeaponsFromEquipment[i];
                    return true;
                }
            }
        }

        return false;
    }
}
