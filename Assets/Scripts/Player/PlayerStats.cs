using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public GameObject Player;
    private int maxGold = 1000000;
    private int _goldAmount;
    public static event System.Action<int> OnGoldChanged;
    public int GoldAmount
    {
        get => _goldAmount;
        set
        {
            // 들어온 값(value)이 maxGold보다 크면 maxGold로 고정, 
            // 0보다 작으면 0으로 고정 (음수 방지)
            _goldAmount = (int)Mathf.Clamp(value, 0, maxGold);
            
            // 값이 변했으니 UI 업데이트 알림
            OnGoldChanged?.Invoke(GoldAmount); 
            
            // (선택사항) 만약 최대치에 도달했다면 로그 출력
            if (_goldAmount >= maxGold)
            {
                Debug.Log("골드가 최대치에 도달했습니다!");
            }
        }
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
    public float HealthRegeneration { get; set; }
    public float experienceBonus { get; set; }
    public float projectileSpeedBonus { get; set; }
    public float cooldownReduction { get; set; }
    public float LuckBonus { get; set; }
    public float KnockBackBonus { get; set; }
    public float ArmorBonus { get; set; }
    public float DoubleDamageChance { get; set; }

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

        while (currentXP >= requiredXP)
        {
            LevelUp();
        }

        UpdateExpBar();
    }

    void LevelUp()
    {
        currentXP -= (int)requiredXP;
        level++;
        requiredXP *= 1.5f;

        if (PlayerStatsCalculate.Instance != null)
            PlayerStatsCalculate.Instance.LevelUpBonus(level - 1);

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
        if(MenuButtonController.Instance.Inventory == true)
        {
            MenuButtonController.Instance.Inventory = false;
			MenuButtonController.Instance.InventoryObject.SetActive(false);
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