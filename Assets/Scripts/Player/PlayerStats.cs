using System.IO;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;

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
        goldAmount = (long)Mathf.Max(0, value);
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
    public int currentXP = 0;
    public float requiredXP = 50;
    public float AttackSpeedBonnes;
    [SerializeField] private Slider ExpBar;
    public float DamageBonus { get; set; }
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
     private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "Q", "aa", "ab", "ac" };
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
        string GoldText_Format = Format(GoldAmount);
        GoldAmountText.text = $"{GoldText_Format}";
    }
    public string Format(double value)
    {
        // 음수 처리 (필요한 경우)
        if (value < 0) return "-" + Format(-value);

        // 1000 미만은 그대로 정수로 표시 (예: 999 -> 999)
        if (value < 1000)
        {
            return value.ToString("0"); 
        }

        // 자릿수 계산 (로그 활용)
        // 1000(10^3) -> index 1 (K)
        // 1,000,000(10^6) -> index 2 (M)
        int zeroCount = (int)Mathf.Log10((float)value);
        int index = zeroCount / 3;

        // 정의된 단위 범위를 넘어가면 마지막 단위 사용
        if (index >= Suffixes.Length)
        {
            index = Suffixes.Length - 1;
        }

        // 해당 단위로 나누기
        double divisor = Mathf.Pow(1000, index);
        double shortValue = value / divisor;

        // 포맷팅:
        // "0.#" : 소수점 첫째 자리까지 표시하되, .0이면 생략합니다. (1.5K, 10K)
        // "0.##" : 소수점 둘째 자리까지 표시 (1.25M)
        return shortValue.ToString("0.#") + Suffixes[index];
    }
    
    // int나 float 등을 위한 오버로딩 (편의성)
    public string Format(float value) => Format((double)value);
    public string Format(int value) => Format((double)value);
    public string Format(long value) => Format((double)value);


    // --- New Save/Load Integration ---

    public PlayerStatsData GetSaveData()
    {
        return new PlayerStatsData(this);
    }
    public void ResetDataNotGold()
    {
        StageCleared = 0;
        CharacterID = 0;
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
            Debug.Log("No player stats data found. Using default values.");
        }
        else
        {
            // Load values from data
            SetGold(data.goldAmount);
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

        while (currentXP >= requiredXP)
        {
            LevelUp();
        }

        PlayerStatsCalculate.Instance.LevelUpBonus(level - 1);

        UpdateExpBar();
    }

    void LevelUp()
    {
        currentXP -= (int)requiredXP;
        level++;
        requiredXP *= 1.5f;

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
        if(MenuButtonController.Instance == true)
        {
            MenuButtonController.Instance.CloseAllMenus();
        }
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.AddPendingUpgrade();
        }
    }

    public void ApplyPowerUps()
    {
        if (PlayerStatsCalculate.Instance == null) return;

        foreach (var powerUp in powerUps)
        {
            if (powerUp.CurrentLevel > 0 && powerUp.CurrentLevel <= powerUp.upgradeValues.Length)
            {
                float upgradeValue = powerUp.upgradeValues[powerUp.CurrentLevel - 1];
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