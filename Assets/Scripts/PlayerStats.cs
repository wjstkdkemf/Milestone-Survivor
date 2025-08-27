using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public int GoldAmount;
    public int StageCleared;
    public int CharacterID;
    public int level = 1;
    public int currentXP = 0;
    public float requiredXP = 50; // XP needed to level up
    public float AttackSpeedBonnes;
    [SerializeField] private Slider ExpBar;
    public float DamageBonus = 0;
    public float SpeedBonus = 0;
    public float HealthRegeneration;
    public float experienceBonus = 0;
    public float projectileSpeedBonus = 0;
    public float cooldownReduction = 0;
    public float LuckBonus;
    public float KnockBackBonus;
    public float ArmorBonus;
    public float DoubleDamageChance;

    public TMP_Text GoldAmountText;
    public List<PowerUpScriptableObject> powerUps; // List of power-u

    private string saveFilePath;

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "playerStats.json");
        Instance = this;
    }
    private void Start()
    {
        LoadStats();

        if (ExpBar != null)
        {
            ExpBar.maxValue = requiredXP;
            ExpBar.value = currentXP;
        }
        // Apply all power-up effects at the start of the game
        Invoke("ApplyPowerUps", .2f);

    }
    private void FixedUpdate()
    {
        if (GoldAmountText != null)
            GoldAmountText.text = $"{GoldAmount}";
    }
    public void AddXP(int amount)
    {
        currentXP += amount + Mathf.RoundToInt(experienceBonus);
        ExpBar.value = currentXP;
        if (currentXP >= requiredXP)
        {
            LevelUp();
        }
    }

    void LevelUp() // 경험치 관련 부분
    {
        level++;
        currentXP = 0;
        requiredXP *= 1.5f; // Increase XP needed for next level
        ExpBar.maxValue = requiredXP;
        ExpBar.value = 0;
        //ShowUpgradeMenu(); // Trigger upgrade menu

        if (level < 10)
        {
            LevelUpLess10();
        }
        else if (level < 100)
        {
            LevelUpLess100();
        }
        else if (level < 1000)
        {
            LevelUpLess1000();
        }
    }

    public void ShowUpgradeMenu()
    {
        // Show UI with upgrade choices
        UpgradeManager.Instance.DisplayUpgrades();
    }

    void ApplyStartingOptionBonuses()
    {
        switch (CharacterID)
        {
            case 0:
                if (level % 10 == 0 && DamageBonus < 50) // Cap at +50%
                {
                    DamageBonus += 10;
                    Debug.Log($"Damage bonus: {DamageBonus}%");
                }
                break;

            case 1:
                if (level % 5 == 0 && experienceBonus < 30) // Cap at +30 XP
                {
                    experienceBonus += 10;
                    Debug.Log($"Experience bonus: +{experienceBonus}");
                }
                break;

            case 2:
                if (level % 5 == 0 && projectileSpeedBonus < 30) // Cap at +30% speed
                {
                    projectileSpeedBonus += 10;
                    Debug.Log($"Projectile speed bonus: {projectileSpeedBonus}%");
                }
                break;

            case 3:
                if (level % 10 == 0 && cooldownReduction < 40) // Cap at -40% cooldown
                {
                    cooldownReduction += 10;
                    Debug.Log($"Cooldown reduction: -{cooldownReduction}%");
                }
                break;
        }
    }
    public void ApplyPowerUps()
    {
        foreach (var powerUp in powerUps)
        {
            if (powerUp.CurrentLevel <= 0 || powerUp.CurrentLevel > powerUp.upgradeValues.Length)
            {
                Debug.LogWarning($"Invalid CurrentLevel for power-up: {powerUp.powerUpName} , {powerUp.upgradeValues.Length} , {powerUp.CurrentLevel}");
                continue;
            }

            float upgradeValue = powerUp.upgradeValues[powerUp.CurrentLevel - 1];

            switch (powerUp.powerUpType)
            {
                case PowerUpType.MaxHealth:
                    // Assuming MaxHealthBonus is a stat you want to modify
                    Debug.Log($"Max Health upgrade: {upgradeValue}");
                    break;
                case PowerUpType.Damage:
                    DamageBonus += upgradeValue;
                    break;
                case PowerUpType.Armor:
                    ArmorBonus += upgradeValue;
                    break;
                case PowerUpType.KnockBack:
                    KnockBackBonus += upgradeValue;
                    break;
                case PowerUpType.HealthRegeneration:
                    HealthRegeneration += upgradeValue;
                    break;
                case PowerUpType.DobleDamageChance:
                    DoubleDamageChance += upgradeValue;
                    break;
                case PowerUpType.CooldownReduction:
                    cooldownReduction += upgradeValue;
                    break;
                case PowerUpType.XPBoost:
                    experienceBonus += upgradeValue;
                    break;
                case PowerUpType.luckBoost:
                    LuckBonus += upgradeValue;
                    break;
                case PowerUpType.MovementSpeed:
                    SpeedBonus += upgradeValue;
                    break;
                default:
                    Debug.LogWarning($"Unhandled power-up type: {powerUp.powerUpType}");
                    break;
            }
        }
    }
    public void InitializeCharacterStats(CharacterScriptableObject character)
    {
        if (character == null)
        {
            Debug.LogWarning("CharacterScriptableObject is null! Cannot initialize player stats.");
            return;
        }

        // Assign values from CharacterScriptableObject to PlayerStats
        DamageBonus += character.Damage;
        SpeedBonus += character.MovementSpeed;
        HealthRegeneration += character.HealthRegeneration;
        experienceBonus += character.XPBoost;
        cooldownReduction += character.CooldownReduction;
        LuckBonus += character.LuckBoost;
        ArmorBonus += character.Armor;
        DoubleDamageChance += character.DobleDamageChance;
    }

    public void ResetStats()
    {
        level = 1;
        currentXP = 0;
        requiredXP = 50;

        SaveStats();
    }

    public void SaveStats()
    {
        PlayerData data = new PlayerData
        {
            _GoldAmount = GoldAmount,
            _StageCleared = StageCleared,
            _CharacterID = CharacterID,

            _level = level,
            _currentXP = currentXP,
            _requiredXP = requiredXP
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Player stats saved to {saveFilePath}");
    }

    public void ClearStats()
    {
        PlayerData data = new PlayerData
        {
            _GoldAmount = 0,
            _StageCleared = 0,
            _CharacterID = 1,

            _level = 1,
            _currentXP = 0,
            _requiredXP = 0
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Player stats saved to {saveFilePath}");
    }

    public void AddCoin(int Amount)
    {
        GoldAmount += Amount;
    }
    /// <summary>
    /// Load player stats from a JSON file.
    /// </summary>
    public void LoadStats()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);// 요기 건들기
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            GoldAmount = data._GoldAmount;
            StageCleared = data._StageCleared;
            CharacterID = data._CharacterID;

            level = data._level;
            currentXP = data._currentXP;
            requiredXP = data._requiredXP;
            Debug.Log($"Player stats loaded from {saveFilePath}");
        }
        else
        {
            GoldAmount = 0; // Default value
            Debug.Log("No save file found. Using default values.");
        }

        if (level < 1)
        {
            // --- 신규 유저를 위한 초기 설정 ---
            level = 1;
            currentXP = 0;
            requiredXP = 50; // 예: 레벨 1의 요구 경험치는 50
        }
    }

    public void LevelUpLess10()
    {
        if (level == 2 || level == 5 || level == 8)
        {
            MilestoneLevelUp();
        }
    }
    public void LevelUpLess100()
    {
        if (level == 10)
        {
            MilestoneLevelUp();
        }

        if (level % 10 == 0)
        {
            MilestoneLevelUp();
        }
    }
    public void LevelUpLess1000()
    {
        if (level == 100)
        {
            MilestoneLevelUp();
        }

        if (level % 100 == 0)
        {
            MilestoneLevelUp();
        }
    }

    public void MilestoneLevelUp()
    {
        ShowUpgradeMenu();
    }

    [System.Serializable]
    public class PlayerData
    {
        public int _GoldAmount;
        public int _StageCleared;
        public int _CharacterID;

        public int _level;
        public int _currentXP;
        public float _requiredXP;
    }
}
