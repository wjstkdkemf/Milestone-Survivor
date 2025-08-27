using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public List<UpgradeScriptableObject> UpgadeToSpawn;
    public GameObject[] UpgadeUiObject;
    [SerializeField] private GameObject UpgradeObject;
    private List<UpgradeScriptableObject> spawnedUpgades = new List<UpgradeScriptableObject>();

    [SerializeField] private GameObject TurretObject;
    [SerializeField] private GameObject OrbsObject;
    [SerializeField] private GameObject PlayerObject;
    [SerializeField] private GameObject PetObject;
    [SerializeField] private GameObject RandomExplosionsObject;
    [SerializeField] private GameObject SwordSlashObject;
    [SerializeField] private GameObject LightningObject;
    [SerializeField] private GameObject KnifeThrowingAbility;

    public float TurretBonus = 0;
    public float OrbsBonus = 0;
    public float PlayerBonus = 0;
    public float PetBonus = 0;
    public float RandomExplosionsBonus = 0;
    public float SwordSlashBonus = 0;
    public float LightningBonus = 0;
    public float KnifeThrowingBonus = 0;

    private string saveUpgradeFilePath;
    private void Awake()
    {
        saveUpgradeFilePath = Path.Combine(Application.persistentDataPath, "PlayerUpgradeData.json");
        Instance = this;
    }
    private void Start()
    {
        foreach (UpgradeScriptableObject info in UpgadeToSpawn)
        {
            info.Points = 0;
        }

        // Check if the current scene is a combat scene by looking for a tagged object
        if (GameObject.FindGameObjectWithTag("CombatScene") != null)
        {
            Debug.Log("전투 씬을 감지하여 저장된 업그레이드를 로드합니다.");
            LoadUpgrades();
            ApplyBonusesToObjects();
        }
        else
        {
            Debug.Log("전투 씬이 아니므로 업그레이드를 로드하지 않습니다.");
        }
    }

    private void ApplyBonusesToObjects()
    {
        if (TurretObject != null)
        {
            TurretObject.GetComponent<Turret>().bulletNumber = (int)TurretBonus;
        }
        if (OrbsObject != null)
        {
            OrbsObject.GetComponent<Orbs>().orbCount = (int)OrbsBonus;
            OrbsObject.GetComponent<Orbs>().SpawnOrbs();
        }
        if (RandomExplosionsObject != null)
        {
            RandomExplosionsObject.GetComponent<RandomSpawner>().SpawnNumber = (int)RandomExplosionsBonus;
        }
        if (SwordSlashObject != null)
        {
            SwordSlashObject.GetComponent<SowrdSlash>().SlashCount = (int)SwordSlashBonus;
        }
        if (LightningObject != null)
        {
            LightningObject.GetComponent<AbilityLightning>().LightningNumber = (int)LightningBonus;
        }
        if (KnifeThrowingAbility != null)
        {
            KnifeThrowingAbility.GetComponent<KnifeThrowingAbility>().KnifeCount = (int)KnifeThrowingBonus;
        }
    }

    public void ResetUpgrade() // New Game 버튼에서 실행
    {
        TurretBonus = 0;
        OrbsBonus = 0;
        PlayerBonus = 0;
        PetBonus = 0;
        RandomExplosionsBonus = 0;
        SwordSlashBonus = 0;
        LightningBonus = 0;
        KnifeThrowingBonus = 0;


        SaveUpgrade();
    }

    private void Update()
    {

    }
    void Generate()
    {

        int totalSpawnChance = 0;

        // Calculate the total spawn chance for all objects
        foreach (UpgradeScriptableObject spawnInfo in UpgadeToSpawn)
        {
            totalSpawnChance += spawnInfo.Chance;
        }

        // Generate a random value within the total spawn chance
        int randomValue = Random.Range(0, totalSpawnChance);

        // Determine which object should be spawned
        foreach (UpgradeScriptableObject spawnInfo in UpgadeToSpawn)
        {
            if (randomValue < spawnInfo.Chance)
            {
                UpgradeScriptableObject objectToSpawn = spawnInfo;

                // Check if the object has not been spawned before
                if (!spawnedUpgades.Contains(objectToSpawn))
                {

                    UpgadeUiObject[spawnedUpgades.Count].GetComponent<UpgradeUi>().SetInfo(objectToSpawn);
                    //  Debug.Log("Upgade" + objectToSpawn.Title);
                    spawnedUpgades.Add(objectToSpawn);
                    break;
                }
            }
            else
            {
                randomValue -= spawnInfo.Chance;
            }
        }
    }

    public void SaveUpgrade()
    {
        UpgradeData data = new UpgradeData
        {
            _TurretBonus = TurretBonus,
            _OrbsBonus = OrbsBonus,
            _PlayerBonus =  PlayerBonus,
            _PetBonus = PetBonus,
            _RandomExplosionsBonus = RandomExplosionsBonus,
            _SwordSlashBonus = SwordSlashBonus,
            _LightningBonus = LightningBonus,
            _KnifeThrowingBonus = KnifeThrowingBonus
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveUpgradeFilePath, json);
        Debug.Log($"Player Upgrade saved to {saveUpgradeFilePath}");
    }


    public void DisplayUpgrades()
    {
        /*foreach (UpgradeScriptableObject upgradeInfo in UpgadeToSpawn)
        {
            CheckForMaxUpgade(upgradeInfo);
        }
        UpgradeObject.SetActive(true);
        GameManager.Instance.Pause = true;
        for (int i = 0; i < UpgadeUiObject.Length; i++)
        {
            Generate();
        }*/
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
        Debug.Log("업데이트창 종료");
        
        //AudioManager.instance.PlaySound("Upgrade");
        UpgradeObject.SetActive(false);
        spawnedUpgades.Clear();
    }
    public void Test()
    {
        Debug.Log("it's working");
    }

    public void ShootProjectile()
    {

        if (TurretObject != null)
        {
            //   TurretObject.SetActive(true);
            TurretBonus++;
            TurretObject.GetComponent<Turret>().bulletNumber++;
        }
    }
    public void RandomExplosions()
    {

        if (RandomExplosionsObject != null)
        {
            RandomExplosionsBonus++;
            RandomExplosionsObject.GetComponent<RandomSpawner>().SpawnNumber++;
        }
    }
    public void KnifeProjectile()
    {
        KnifeThrowingBonus++;
        KnifeThrowingAbility.GetComponent<KnifeThrowingAbility>().KnifeCount++;
    }
    public void NewOrb()
    {

        if (OrbsObject != null)
        {
            OrbsObject.GetComponent<Orbs>().orbCount++;
            OrbsObject.GetComponent<Orbs>().SpawnOrbs();
        }

    }
    public void SpawnPet()
    {

        if (PlayerObject != null && PetObject != null)
        {
            GameObject PlayerPet = Instantiate(PetObject, PlayerObject.transform.position, Quaternion.identity);
        }


    }
    public void AddHealth()
    {

        PlayerObject.GetComponent<PlayerHealth>().MaxHealth *= 1.2f;
    }
    public void AddSpeed()
    {
        PlayerStats.Instance.SpeedBonus += 5f;
    }
    public void AddDamge()
    {
        PlayerStats.Instance.DamageBonus += 5f;

    }
    public void Heal()
    {

        PlayerObject.GetComponent<PlayerHealth>().Heal(50);
    }
    public void AttackSpeed()
    {
        PlayerStats.Instance.AttackSpeedBonnes += 5;

    }
    public void SwordSlash()
    {
        if (SwordSlashObject != null)
        {
            SwordSlashBonus++;
            SwordSlashObject.GetComponent<SowrdSlash>().SlashCount++;
        }
    }
    public void ExperienceBonus()
    {
        PlayerStats.Instance.experienceBonus += 5;
    }
    public void LightningBolt()
    {
        if (LightningObject != null)
        {
            //   TurretObject.SetActive(true);
            LightningBonus++;
            LightningObject.GetComponent<AbilityLightning>().LightningNumber++;
        }
    }
    public void LoadUpgrades()
    {
        if (File.Exists(saveUpgradeFilePath))
        {
            string json = File.ReadAllText(saveUpgradeFilePath);// 요기 건들기
            UpgradeData data = JsonUtility.FromJson<UpgradeData>(json);
            TurretBonus = data._TurretBonus;
            OrbsBonus = data._OrbsBonus;
            PlayerBonus = data._PlayerBonus;
            PetBonus = data._PetBonus;
            RandomExplosionsBonus = data._RandomExplosionsBonus;
            SwordSlashBonus = data._SwordSlashBonus;
            LightningBonus = data._LightningBonus;
            KnifeThrowingBonus = data._KnifeThrowingBonus;

            Debug.Log($"Player stats loaded from {saveUpgradeFilePath}");
        }
        else
        {
            //GoldAmount = 0; // Default value
            Debug.Log("No save upgrade file found. Using default values.");
        }
    }

    public void ExperienceBoost()
    {
        PlayerStats.Instance.experienceBonus += 5;
    }

    public void CheckForMaxUpgade(UpgradeScriptableObject info)
    {
        if (info.Points == info.MaxPoints)
            UpgadeToSpawn.Remove(info);
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
    }

}