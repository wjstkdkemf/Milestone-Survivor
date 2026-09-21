using System.IO;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public GameObject Player;
    private long maxGold = 1000000;
    [SerializeField]private long goldAmount;
    public long GoldAmount => goldAmount;
    public event Action<long> OnGoldChanged;
    public void AddGold(long amount)
    {
        if (amount <= 0)
            return;

        goldAmount += amount;
        OnGoldChanged?.Invoke(goldAmount);
    }

    public bool TrySpendGold(long cost)
    {
        if (cost <= 0)
            return true;

        if (goldAmount < cost)
            return false;

        goldAmount -= cost;
        OnGoldChanged?.Invoke(goldAmount);
        return true;
    }

    public void SetGold(long value)
    {
        goldAmount = Math.Max(0L, value);
        OnGoldChanged?.Invoke(goldAmount);
    }

    public string Format_Gold(long value)
    {
        if (value >= 1_000_000_000)
            return $"{value / 1_000_000_000f:0.##}B";

        if (value >= 1_000_000)
            return $"{value / 1_000_000f:0.##}M";

        if (value >= 1_000)
            return $"{value / 1_000f:0.##}K";

        return value.ToString();
    }
    public int StageCleared;
    public int CharacterID;
    public int level = 1;
    public long currentXP = 0;
    public double requiredXP = 50;
    // Exactly representable in both long and double, with room for incoming XP.
    public const long MaxRequiredXP = 1L << 62;
    public float AttackSpeedBonnes;
    [SerializeField] private Slider ExpBar;
    public long DamageBonus { get; set; }
    public float SpeedBonus { get; set; }
    public float projectileSpeedBonus { get; set; }
    public float HealthRegeneration { get; set; }
    public float experienceBonus { get; set; }
    public float cooldownReduction { get; set; }
    public float LuckBonus { get; set; }
    public float KnockBackBonus { get; set; }
    public float ArmorBonus { get; set; }
    private float _doubleDamageChance;
    public float DoubleDamageChance { 
        get { return _doubleDamageChance; }
        set { _doubleDamageChance = Mathf.Clamp(value, 0.0f, 1.0f); }}
    public float NormalArmorRatio = 1000.0f;

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

        if(Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        // Update UI and apply stats for the new scene
    }
    public void init()
    {
        ResetDataNotGold();
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
           RotateGoldText();
    }
    private void RotateGoldText()
    {
        string GoldText_Format = NumberFormatter.Format(GoldAmount);
        GoldAmountText.text = $"{GoldText_Format}";
    }


    // --- New Save/Load Integration ---

    public PlayerStatsData GetSaveData()
    {
        return new PlayerStatsData(this);
    }
    public void ResetDataNotGold()
    {
        StageCleared = 0;
        level = 1;
        currentXP = 0;
        requiredXP = 50;

        if (PlayerStatsCalculate.Instance != null && Player != null)
        {
            PlayerStatsCalculate.Instance.ResetBonuses();
        }
    }

    public void LoadData(PlayerStatsData data)
    {
        if (data == null)
        {
            // Load default values for a new game
            SetGold(0);
            StageCleared = 0;
            CharacterID = 0;
            level = 1;
            currentXP = 0;
            requiredXP = 50;
            DevLog.Log("No player stats data found. Using default values.");
        }
        else
        {
            // Load values from data
            SetGold(data.goldAmount);
            StageCleared = data.stageCleared;
            CharacterID = data.characterID;
            level = Math.Max(1, data.level);
            currentXP = Math.Max(0L, data.currentXP);
            requiredXP = NormalizeRequiredXP(data.requiredXP);
            DevLog.Log("Player stats loaded.");
        }
        UpdateExpBar();
    }

    private void UpdateExpBar()
    {
        if (ExpBar != null)
        {
            ExpBar.wholeNumbers = false;
            ExpBar.minValue = 0f;
            ExpBar.maxValue = 1f;
            ExpBar.value = (float)(currentXP / NormalizeRequiredXP(requiredXP));
        }
    }

    // --- Existing Game Logic ---

    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        requiredXP = NormalizeRequiredXP(requiredXP);
        double bonus = float.IsNaN(experienceBonus) || float.IsInfinity(experienceBonus)
            ? 0d : Math.Round(experienceBonus);
        long gainedXP = (long)Math.Min(MaxRequiredXP, Math.Max(0d, amount + bonus));
        currentXP = Math.Max(0L, currentXP);
        currentXP += Math.Min(gainedXP, long.MaxValue - currentXP);

        while (level < int.MaxValue && currentXP >= (long)Math.Ceiling(requiredXP))
        {
            LevelUp();
        }

        if (PlayerStatsCalculate.Instance != null)
            PlayerStatsCalculate.Instance.LevelUpBonus(level - 1);

        UpdateExpBar();
    }

    void LevelUp()
    {
        currentXP -= (long)Math.Ceiling(requiredXP);
        level++;
        requiredXP = NormalizeRequiredXP(requiredXP * 1.5d);

        if (level < 100)
        {
            LevelUpLess10();
        }
        else if (level < 1000)
        {
            LevelUpLess100();
        }
        else if (level < 10000)
        {
            LevelUpLess1000();
        }
    }

    public void ShowUpgradeMenu()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.AddPendingUpgrade();
        }
    }

    public static double NormalizeRequiredXP(double value)
    {
        if (double.IsNaN(value) || value <= 0d) return 50d;
        return Math.Min(MaxRequiredXP, value);
    }

    public void ApplyPowerUps()
    {
        if (PlayerStatsCalculate.Instance == null) return;
        if (powerUps == null) return;

        foreach (var powerUp in powerUps)
        {
            if (powerUp == null || powerUp.upgradeValues == null)
                continue;

            if (powerUp.CurrentLevel > 0 && powerUp.CurrentLevel < powerUp.upgradeValues.Length)
            {
                float upgradeValue = powerUp.upgradeValues[powerUp.CurrentLevel];
                PlayerStatsCalculate.Instance.AddPowerUpBonus(powerUp.powerUpType, upgradeValue);
            }
        }
    }
    public void LevelUpLess10()
    {
        if (level < 100)
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
