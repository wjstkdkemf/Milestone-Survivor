using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public List<UpgradeScriptableObject> UpgadeToSpawn;
    public GameObject[] UpgadeUiObject;
    [SerializeField] private GameObject UpgradeObject;
    private List<UpgradeScriptableObject> spawnedUpgades = new List<UpgradeScriptableObject>();

    [Header("Ability GameObjects")]
    [SerializeField] private GameObject TurretObject;
    [SerializeField] private GameObject OrbsObject;
    [SerializeField] private GameObject PlayerObject;
    [SerializeField] private GameObject PetObject;
    [SerializeField] private GameObject RandomExplosionsObject;
    [SerializeField] private GameObject SwordSlashObject;
    [SerializeField] private GameObject LightningObject;
    [SerializeField] private GameObject KnifeThrowingAbility;
    [SerializeField] private GameObject MeteorAbility; // Meteor GameObject field
    [SerializeField] private GameObject LightningSparkObject;

    [Header("Ability Levels")]
    public float TurretBonus = 0;
    public float OrbsBonus = 0;
    public float PlayerBonus = 0;
    public float PetBonus = 0;
    public float RandomExplosionsBonus = 0;
    public float SwordSlashBonus = 0;
    public float LightningBonus = 0;
    public float KnifeThrowingBonus = 0;
    public float MeteorAbilityBonus = 0; // Meteor level field
    public float LightningSparkBonus = 0;

    private string saveUpgradeFilePath;

    // Job Class System
    private bool isJobClassSet = false;
    public enum JobClass { None, Mage, Shooter }
    private JobClass currentJobClass = JobClass.None;
    private Dictionary<JobClass, List<UpgradeScriptableObject.UpgardeEnum>> jobClassSkills = new Dictionary<JobClass, List<UpgradeScriptableObject.UpgardeEnum>>();

    private bool LoadData = false;

    private void Awake()
    {
        saveUpgradeFilePath = Path.Combine(Application.persistentDataPath, "PlayerUpgradeData.json");
        Instance = this;
        InitializeJobClasses();
    }

    private void Start()
    {
        if (LoadData)
        {
            if (!PersistentDataManager.Instance.upgradeChancesInitialized)
            {
                foreach (UpgradeScriptableObject upgrade in UpgadeToSpawn)
                {
                    PersistentDataManager.Instance.initialUpgradeChances[upgrade.Upgarde] = upgrade.Chance;
                }
                PersistentDataManager.Instance.upgradeChancesInitialized = true;
            }

            LoadUpgrades();
            ApplyBonusesToObjects();
            SyncChancesFromPersistentData();
            SyncPointsFromPersistentData();
        }
        else if (GameObject.FindGameObjectWithTag("Village") != null)
        {
            ResetRunData();
        }

        // Initially, set combat state to false
        SetCombatState(false);
    }

    public void SetCombatState(bool isActive)
    {
        if (TurretObject != null) TurretObject.SetActive(isActive);
        if (OrbsObject != null) OrbsObject.SetActive(isActive);
        if (PetObject != null) PetObject.SetActive(isActive);
        if (RandomExplosionsObject != null) RandomExplosionsObject.SetActive(isActive);
        if (SwordSlashObject != null) SwordSlashObject.SetActive(isActive);
        if (LightningObject != null) LightningObject.SetActive(isActive);
        if (KnifeThrowingAbility != null) KnifeThrowingAbility.SetActive(isActive);
        if (MeteorAbility != null) MeteorAbility.SetActive(isActive);
        if (LightningSparkObject != null) LightningSparkObject.SetActive(isActive);
    }

    public void SyncChancesFromPersistentData()
    {
        if (!PersistentDataManager.Instance.currentChancesInitialized)
        {
            PersistentDataManager.Instance.currentUpgradeChances.Clear();
            foreach (var upgrade in UpgadeToSpawn)
            {
                PersistentDataManager.Instance.currentUpgradeChances[upgrade.Upgarde] = upgrade.Chance;
            }
            PersistentDataManager.Instance.currentChancesInitialized = true;
        }
        else
        {
            foreach (var upgrade in UpgadeToSpawn)
            {
                if (PersistentDataManager.Instance.currentUpgradeChances.ContainsKey(upgrade.Upgarde))
                {
                    upgrade.Chance = PersistentDataManager.Instance.currentUpgradeChances[upgrade.Upgarde];
                }
            }
        }
    }

    public void SyncPointsFromPersistentData()
    {
        if (!PersistentDataManager.Instance.currentPointsInitialized)
        {
            PersistentDataManager.Instance.currentUpgradePoints.Clear();
            foreach (var upgrade in UpgadeToSpawn)
            {
                PersistentDataManager.Instance.currentUpgradePoints[upgrade.Upgarde] = 0;
            }
            PersistentDataManager.Instance.currentPointsInitialized = true;
        }
        else
        {
            foreach (var upgrade in UpgadeToSpawn)
            {
                if (PersistentDataManager.Instance.currentUpgradePoints.ContainsKey(upgrade.Upgarde))
                {
                    upgrade.Points = PersistentDataManager.Instance.currentUpgradePoints[upgrade.Upgarde];
                }
            }
        }
    }

    public void SaveCurrentChancesToPersistentData()
    {
        foreach (var upgrade in UpgadeToSpawn)
        {
            PersistentDataManager.Instance.currentUpgradeChances[upgrade.Upgarde] = upgrade.Chance;
        }
        PersistentDataManager.Instance.SaveCurrentChances();
    }

    public void SaveCurrentPointsToPersistentData()
    {
        foreach (var upgrade in UpgadeToSpawn)
        {
            PersistentDataManager.Instance.currentUpgradePoints[upgrade.Upgarde] = upgrade.Points;
        }
        PersistentDataManager.Instance.SaveCurrentPoints();
    }

    public void ResetRunData()
    {
        // Reset Chances
        foreach (UpgradeScriptableObject upgrade in UpgadeToSpawn)
        {
            if (PersistentDataManager.Instance.initialUpgradeChances.ContainsKey(upgrade.Upgarde))
            {
                upgrade.Chance = PersistentDataManager.Instance.initialUpgradeChances[upgrade.Upgarde];
            }
        }
        SaveCurrentChancesToPersistentData();

        // Reset Points
        foreach (UpgradeScriptableObject upgrade in UpgadeToSpawn)
        {
            upgrade.Points = 0;
        }
        SaveCurrentPointsToPersistentData();

        // Reset Job Class
        isJobClassSet = false;
        currentJobClass = JobClass.None;
    }

    private void ApplyBonusesToObjects()
    {
        if (TurretObject != null && TurretBonus > 0)
        {
            TurretObject.GetComponent<Turret>().bulletNumber = (int)TurretBonus;
        }
        if (OrbsObject != null && OrbsBonus > 0)
        {
            OrbsObject.GetComponent<Orbs>().orbCount = (int)OrbsBonus;
            OrbsObject.GetComponent<Orbs>().SpawnOrbs();
        }
        if (RandomExplosionsObject != null && RandomExplosionsBonus > 0)
        {
            RandomExplosionsObject.GetComponent<RandomSpawner>().SpawnNumber = (int)RandomExplosionsBonus;
        }
        if (SwordSlashObject != null && SwordSlashBonus > 0)
        {
            SwordSlashObject.GetComponent<SowrdSlash>().SlashCount = (int)SwordSlashBonus;
        }
        if (LightningObject != null && LightningBonus > 0)
        {
            LightningObject.GetComponent<AbilityLightning>().LightningNumber = (int)LightningBonus;
        }
        if (KnifeThrowingAbility != null && KnifeThrowingBonus > 0)
        {
            KnifeThrowingAbility.GetComponent<KnifeThrowingAbility>().KnifeCount = (int)KnifeThrowingBonus;
        }
        if (MeteorAbility != null && MeteorAbilityBonus > 0)
        {
            MeteorAbility.GetComponent<MeteorStrikeAbility>().MeteorCount = (int)MeteorAbilityBonus;
        }
        if (LightningSparkObject != null && LightningSparkBonus > 0)
        {
            LightningSparkObject.GetComponent<LightningSparkAbility>().bounces = (int)LightningSparkBonus;
        }
    }

    // This function will be called by the upgrade card's button


    public void ResetUpgrade()
    {
        TurretBonus = 0;
        OrbsBonus = 0;
        PlayerBonus = 0;
        PetBonus = 0;
        RandomExplosionsBonus = 0;
        SwordSlashBonus = 0;
        LightningBonus = 0;
        KnifeThrowingBonus = 0;
        MeteorAbilityBonus = 0; // Reset meteor level
        LightningSparkBonus = 0;
        SaveUpgrade();
    }

    public void SaveUpgrade()
    {
        UpgradeData data = new UpgradeData
        {
            _TurretBonus = TurretBonus,
            _OrbsBonus = OrbsBonus,
            _PlayerBonus = PlayerBonus,
            _PetBonus = PetBonus,
            _RandomExplosionsBonus = RandomExplosionsBonus,
            _SwordSlashBonus = SwordSlashBonus,
            _LightningBonus = LightningBonus,
            _KnifeThrowingBonus = KnifeThrowingBonus,
            _MeteorAbilityBonus = MeteorAbilityBonus, // Save meteor level
            _LightningSparkBonus = LightningSparkBonus,
            _currentJobClass = currentJobClass
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveUpgradeFilePath, json);
    }

    public void LoadUpgrades()
    {
        if (File.Exists(saveUpgradeFilePath))
        {
            string json = File.ReadAllText(saveUpgradeFilePath);
            UpgradeData data = JsonUtility.FromJson<UpgradeData>(json);
            TurretBonus = data._TurretBonus;
            OrbsBonus = data._OrbsBonus;
            PlayerBonus = data._PlayerBonus;
            PetBonus = data._PetBonus;
            RandomExplosionsBonus = data._RandomExplosionsBonus;
            SwordSlashBonus = data._SwordSlashBonus;
            LightningBonus = data._LightningBonus;
            KnifeThrowingBonus = data._KnifeThrowingBonus;
            MeteorAbilityBonus = data._MeteorAbilityBonus; // Load meteor level
            LightningSparkBonus = data._LightningSparkBonus;
            currentJobClass = data._currentJobClass;

            if (currentJobClass != JobClass.None)
            {
                isJobClassSet = true;
            }
        }
    }

    [System.Serializable]
    public class UpgradeData
    {
        public float _TurretBonus;
        public float _OrbsBonus;
        public float _PlayerBonus;
        public float _PetBonus;
        public float _RandomExplosionsBonus;
        public float _SwordSlashBonus;
        public float _LightningBonus;
        public float _KnifeThrowingBonus;
        public float _MeteorAbilityBonus; // Add meteor level to data class
        public float _LightningSparkBonus;
        public JobClass _currentJobClass;

    }

    public void DisplayUpgrades()
    {
        for (int i = UpgadeToSpawn.Count - 1; i >= 0; i--)
        {
            if (UpgadeToSpawn[i].Points >= UpgadeToSpawn[i].MaxPoints)
            {
                UpgadeToSpawn.RemoveAt(i);
            }
        }

        UpgradeObject.SetActive(true);
        GameManager.Instance.Pause = true;

        List<UpgradeScriptableObject> availableUpgrades = new List<UpgradeScriptableObject>(UpgadeToSpawn);

        int upgradesToPick = Mathf.Min(UpgadeUiObject.Length, availableUpgrades.Count);

        for (int i = 0; i < UpgadeUiObject.Length; i++)
        {
            if (i < upgradesToPick)
            {
                int totalSpawnChance = 0;
                foreach (UpgradeScriptableObject spawnInfo in availableUpgrades)
                {
                    totalSpawnChance += spawnInfo.Chance;
                }

                if (totalSpawnChance == 0)
                {
                    UpgadeUiObject[i].SetActive(false);
                    continue;
                }

                int randomValue = Random.Range(0, totalSpawnChance);
                UpgradeScriptableObject chosenUpgrade = null;

                foreach (UpgradeScriptableObject spawnInfo in availableUpgrades)
                {
                    if (randomValue < spawnInfo.Chance)
                    {
                        chosenUpgrade = spawnInfo;
                        break;
                    }
                    else
                    {
                        randomValue -= spawnInfo.Chance;
                    }
                }
                if (chosenUpgrade != null)
                {
                    UpgadeUiObject[i].SetActive(true);
                    UpgadeUiObject[i].GetComponent<UpgradeUi>().SetInfo(chosenUpgrade);
                    spawnedUpgades.Add(chosenUpgrade);
                    availableUpgrades.Remove(chosenUpgrade);
                }
                else
                {
                    UpgadeUiObject[i].SetActive(false);
                }
            }
            else
            {
                UpgadeUiObject[i].SetActive(false);
            }
        }
    }

    public void Close()
    {
        GameManager.Instance.Pause = false;
        UpgradeObject.SetActive(false);
        spawnedUpgades.Clear();

        if (!isJobClassSet)
        {
            CheckAndSetJobClassFromPoints();
        }
    }

    private void ApplyConditionalChance(UpgradeScriptableObject.UpgardeEnum upgradeType)
    {
        // This is where you can define the logic for conditional chances.
        // You can expand this with more complex rules as needed.
        if (upgradeType == UpgradeScriptableObject.UpgardeEnum.Meteor)
        {
            // Increase the chance of LightningSpark
            var lightningSpark = UpgadeToSpawn.FirstOrDefault(u => u.Upgarde == UpgradeScriptableObject.UpgardeEnum.LightningSpark);
            if (lightningSpark != null)
            {
                lightningSpark.Chance += 10;
            }

            // Set the chance of KnifeProjectile to 0
            var knifeProjectile = UpgadeToSpawn.FirstOrDefault(u => u.Upgarde == UpgradeScriptableObject.UpgardeEnum.KnifeProjectile);
            if (knifeProjectile != null)
            {
                knifeProjectile.Chance = 0;
            }
        }
    }

    // --- Job Class System Methods ---

    private void InitializeJobClasses()
    {
        // Define Mage skills
        jobClassSkills[JobClass.Mage] = new List<UpgradeScriptableObject.UpgardeEnum>
        {
            UpgradeScriptableObject.UpgardeEnum.Meteor,
            UpgradeScriptableObject.UpgardeEnum.LightningBolt,
            UpgradeScriptableObject.UpgardeEnum.LightningSpark,
            UpgradeScriptableObject.UpgardeEnum.NewOrb,
            UpgradeScriptableObject.UpgardeEnum.RandomExplosions,
            UpgradeScriptableObject.UpgardeEnum.AddDamge,
            UpgradeScriptableObject.UpgardeEnum.AttackSpeed,
            UpgradeScriptableObject.UpgardeEnum.ExperienceBonus
        };

        // Define Shooter skills
        jobClassSkills[JobClass.Shooter] = new List<UpgradeScriptableObject.UpgardeEnum>
        {
            UpgradeScriptableObject.UpgardeEnum.ShootProjectile,
            UpgradeScriptableObject.UpgardeEnum.KnifeProjectile,
            UpgradeScriptableObject.UpgardeEnum.SwordSlash,
            UpgradeScriptableObject.UpgardeEnum.AddDamge,
            UpgradeScriptableObject.UpgardeEnum.AttackSpeed,
            UpgradeScriptableObject.UpgardeEnum.AddSpeed,
            UpgradeScriptableObject.UpgardeEnum.ExperienceBonus
        };
    }

    private void CheckAndSetJobClassFromPoints()
    {
        // Helper function to get points for a specific upgrade
        int GetPoints(UpgradeScriptableObject.UpgardeEnum type)
        {
            var upgrade = UpgadeToSpawn.FirstOrDefault(u => u.Upgarde == type);
            return upgrade != null ? upgrade.Points : 0;
        }

        // Mage class check
        int magePoints = GetPoints(UpgradeScriptableObject.UpgardeEnum.Meteor) + GetPoints(UpgradeScriptableObject.UpgardeEnum.LightningSpark);
        Debug.Log($"magePoints: {magePoints}");
        if (magePoints >= 2)
        {
            currentJobClass = JobClass.Mage;
        }
        // Shooter class check
        else
        {
            int shooterPoints = GetPoints(UpgradeScriptableObject.UpgardeEnum.ShootProjectile) + GetPoints(UpgradeScriptableObject.UpgardeEnum.KnifeProjectile);
            if (shooterPoints >= 2)
            {
                currentJobClass = JobClass.Shooter;
            }
        }

        if (currentJobClass != JobClass.None)
        {
            Debug.Log($"Job class set to: {currentJobClass}");
            isJobClassSet = true;
            ApplyJobClassChanceBonuses();
            SaveUpgrade(); // Save the job class change
        }
    }

    private void ApplyJobClassChanceBonuses()
    {
        if (currentJobClass == JobClass.None) return;

        List<UpgradeScriptableObject.UpgardeEnum> allowedSkills = jobClassSkills[currentJobClass];

        foreach (var upgrade in UpgadeToSpawn)
        {
            if (allowedSkills.Contains(upgrade.Upgarde))
            {
                upgrade.Chance += 50; // Greatly increase chance for class skills
            }
            else
            {
                upgrade.Chance = 0; // Set chance to 0 for non-class skills
            }
        }
        Debug.Log("Applied job class chance bonuses.");
    }


    // --- Modified Upgrade Methods ---

    public void ShootProjectile() { TurretBonus++; TurretObject.GetComponent<Turret>().bulletNumber++; }
    public void RandomExplosions() { RandomExplosionsBonus++; RandomExplosionsObject.GetComponent<RandomSpawner>().SpawnNumber++; }
    public void KnifeProjectile() { KnifeThrowingBonus++; KnifeThrowingAbility.GetComponent<KnifeThrowingAbility>().KnifeCount++; }
    public void NewOrb() { OrbsBonus++; OrbsObject.GetComponent<Orbs>().orbCount++; OrbsObject.GetComponent<Orbs>().SpawnOrbs(); }
    public void SpawnPet() { if (PlayerObject != null && PetObject != null) { Instantiate(PetObject, PlayerObject.transform.position, Quaternion.identity); } }
    public void AddHealth() { PlayerObject.GetComponent<PlayerHealth>().MaxHealth *= 1.2f; }
    public void AddSpeed() { PlayerStats.Instance.SpeedBonus += 5f; }
    public void AddDamge() { PlayerStats.Instance.DamageBonus += 5f; }
    public void Heal() { PlayerObject.GetComponent<PlayerHealth>().Heal(50); }
    public void AttackSpeed() { PlayerStats.Instance.AttackSpeedBonnes += 5; }
    public void SwordSlash() { SwordSlashBonus++; SwordSlashObject.GetComponent<SowrdSlash>().SlashCount++; }
    public void ExperienceBonus() { PlayerStats.Instance.experienceBonus += 5; }
    public void LightningBolt() { LightningBonus++; LightningObject.GetComponent<AbilityLightning>().LightningNumber++; }
    public void LightningSpark() { LightningSparkBonus++; LightningSparkObject.GetComponent<LightningSparkAbility>().bounces++; }
    public void ExperienceBoost() { PlayerStats.Instance.experienceBonus += 5; }
    public void CheckForMaxUpgade(UpgradeScriptableObject info) { if (info.Points == info.MaxPoints) UpgadeToSpawn.Remove(info); }

    public void Meteor()
    {
        MeteorAbilityBonus++;
        if (MeteorAbility != null)
        {
            MeteorAbility.GetComponent<MeteorStrikeAbility>().MeteorCount = (int)MeteorAbilityBonus;
        }
        ApplyConditionalChance(UpgradeScriptableObject.UpgardeEnum.Meteor);
    }
}