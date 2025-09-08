using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public int GoldAmount;
    public int StageCleared;
    public int CharacterID;
    public int level = 1;
    public int currentXP = 0;
    public float requiredXP = 50;
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
    public List<PowerUpScriptableObject> powerUps; // List of power-ups

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scene changes
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find UI elements in the new scene by tag
        GameObject goldTextObject = GameObject.FindGameObjectWithTag("GoldText");
        if (goldTextObject != null)
        {
            GoldAmountText = goldTextObject.GetComponent<TMP_Text>();
        }

        GameObject expBarObject = GameObject.FindGameObjectWithTag("ExpBar");
        if (expBarObject != null)
        {
            ExpBar = expBarObject.GetComponent<Slider>();
        }

        // Update UI and apply stats for the new scene
        UpdateExpBar();
        ApplyPowerUps();
    }

    private void Start()
    {
        // This check is a safeguard in case the scene is loaded directly
        // without going through the SaveLoadManager flow.
        if (SaveLoadManager.Instance != null && !SaveLoadManager.Instance.IsLoadingFromFile)
        {
             // If this is a fresh game, apply any initial setup here.
             // For now, data is already set to default values upon declaration.
        }
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); // Call once on initial start
    }

    private void FixedUpdate()
    {
        if (GoldAmountText != null)
            GoldAmountText.text = $"{GoldAmount}";
    }

    // --- New Save/Load Integration ---

    public PlayerStatsData GetSaveData()
    {
        return new PlayerStatsData(this);
    }

    public void LoadData(PlayerStatsData data)
    {
        if (data == null)
        {
            // Load default values for a new game
            GoldAmount = 0;
            StageCleared = 0;
            CharacterID = 0;
            level = 1;
            currentXP = 0;
            requiredXP = 50;
            Debug.Log("No player stats data found. Using default values.");
        }
        else
        {
            // Load values from data
            GoldAmount = data.goldAmount;
            StageCleared = data.stageCleared;
            CharacterID = data.characterID;
            level = data.level;
            currentXP = data.currentXP;
            requiredXP = data.requiredXP;
            Debug.Log("Player stats loaded.");
        }
        UpdateExpBar();
    }

    private void UpdateExpBar()
    {
        if (ExpBar != null)
        {
            ExpBar.maxValue = requiredXP;
            ExpBar.value = currentXP;
        }
    }

    // --- Existing Game Logic ---

    public void AddXP(int amount)
    {
        currentXP += amount + Mathf.RoundToInt(experienceBonus);
        if (currentXP >= requiredXP)
        {
            LevelUp();
        }
        UpdateExpBar();
    }

    void LevelUp()
    {
        level++;
        currentXP = 0;
        requiredXP *= 1.5f;
        UpdateExpBar();

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
        UpgradeManager.Instance.DisplayUpgrades();
    }

    public void ApplyPowerUps()
    {
        // This logic remains the same, it reads from the ScriptableObjects
        foreach (var powerUp in powerUps)
        {
            if (powerUp.CurrentLevel <= 0 || powerUp.CurrentLevel > powerUp.upgradeValues.Length)
            {
                continue; // Skip if level is invalid
            }

            float upgradeValue = powerUp.upgradeValues[powerUp.CurrentLevel - 1];

            switch (powerUp.powerUpType)
            {
                case PowerUpType.MaxHealth: break; // Example
                case PowerUpType.Damage: DamageBonus += upgradeValue; break;
                case PowerUpType.Armor: ArmorBonus += upgradeValue; break;
                case PowerUpType.KnockBack: KnockBackBonus += upgradeValue; break;
                case PowerUpType.HealthRegeneration: HealthRegeneration += upgradeValue; break;
                case PowerUpType.DobleDamageChance: DoubleDamageChance += upgradeValue; break;
                case PowerUpType.CooldownReduction: cooldownReduction += upgradeValue; break;
                case PowerUpType.XPBoost: experienceBonus += upgradeValue; break;
                case PowerUpType.luckBoost: LuckBonus += upgradeValue; break;
                case PowerUpType.MovementSpeed: SpeedBonus += upgradeValue; break;
            }
        }
    }

    public void AddCoin(int Amount)
    {
        GoldAmount += Amount;
    }

    public void LevelUpLess10()
    {
        if (level < 10)
        {
            MilestoneLevelUp();
        }
    }
    public void LevelUpLess100()
    {
        if (level % 10 == 0)
        {
            MilestoneLevelUp();
        }
    }
    public void LevelUpLess1000()
    {
        if (level % 100 == 0)
        {
            MilestoneLevelUp();
        }
    }

    public void MilestoneLevelUp()
    {
        ShowUpgradeMenu();
    }
}