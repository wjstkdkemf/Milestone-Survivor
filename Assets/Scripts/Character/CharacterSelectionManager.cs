using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
public class CharacterSelectionManager : MonoBehaviour
{
    public List<CharacterSelectionButton> characterButtons = new List<CharacterSelectionButton>();
    public static CharacterSelectionManager Instance { get; private set; }
  
    public GameObject panle;
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public TMP_Text skillName;
    public TMP_Text skillDescription;
    public StatTableUI baseStatTable;
    public StatTableUI levelUpStatTable;
    public StatPanelUI MainStatPanel;
    public StatPanelUI baseStatPanel;
    public StatPanelUI specialStatPanel;
    public GameObject BuyButtons;
    public GameObject ConfirmButton;
    public Image icon;
    public Image Skillicon;

    private CharacterSelectionButton characterSelectionButton;

    [SerializeField] private List<CharacterScriptableObject> characterList; // List of CharacterScriptableObjects
    private string saveFilePath;
    private CharacterScriptableObject currentCharacterInfo;

    private void Awake()
    {
        // Set the save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, "CharacterData.json");
        LoadCharacterData();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    private void OnDestroy()
    {
        // 내가 현재 활성화된 인스턴스였다면, 파괴될 때 참조를 비워줌
        if (Instance == this)
        {
            Instance = null;
        }
    }
    private void Start()
    {
        foreach(CharacterSelectionButton button in characterButtons)
        {
            button.Initialize(this);
        }
        Invoke("delayedStart", .5f);
        
    }
    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    private void HandleLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        if (currentCharacterInfo != null && characterSelectionButton != null)
            SetInfo(currentCharacterInfo, characterSelectionButton);
    }

    void delayedStart()
    {
        characterButtons[0].Selected();
    }
    public void SetInfo(CharacterScriptableObject info,CharacterSelectionButton button)
    {
        currentCharacterInfo = info;
        characterSelectionButton = button;
        panle.SetActive(true);
        nameText.text = info.GetLocalizedName();
        if(descriptionText != null)
            descriptionText.text = info.GetLocalizedDescription();
        //costText.text = info.costPerLevel.ToString();
        if(icon != null)
            icon.sprite = info.IconSprite;

        List<StatEntry> baseStats = StatEntryFactory.FromCharacterBase(info);

        if(MainStatPanel != null)
        {
            ExtractionMainStat(baseStats);
        }
        
        if (baseStatTable != null)
        {
            baseStatTable.SetStats(baseStats);
        }
        else if (baseStatPanel != null)
        {
            baseStatPanel.SetStats(baseStats);
        }
        List<StatEntry> levelUpStats = StatEntryFactory.FromCharacterLevelUpBonus(info);
        if (levelUpStatTable != null)
        {
            levelUpStatTable.SetStats(levelUpStats);
        }
        else if (specialStatPanel != null)
        {
            specialStatPanel.SetStats(levelUpStats);
        }

        DrawSkillInfo();
        //BuyButtons.SetActive(!info.purchased);
        ConfirmButton.SetActive(info.purchased);
    }
    public void ExtractionMainStat(List<StatEntry> baseStats)
    {
        List<StatEntry> mainStats = new List<StatEntry>();
        mainStats.Add(StatEntry.Header("character.section.main_stats", CharacterLocalization.Get("character.section.main_stats", "main_stats")));

        foreach (StatEntry entry in baseStats)
        {
            if(entry.StatKey == nameof(CharacterScriptableObject.BaseHP) 
            || entry.StatKey == nameof(CharacterScriptableObject.Damage)
            || entry.StatKey == nameof(CharacterScriptableObject.MovementSpeed)
            || entry.StatKey == nameof(CharacterScriptableObject.Armor)
            )
            {
                mainStats.Add(entry);
            }
        }
        MainStatPanel.SetStats(mainStats);
    }
    public void DrawSkillInfo()
    {
        if(Skillicon != null)
            Skillicon.sprite = currentCharacterInfo.startingWeapon.Icon;
        if(skillName != null)
            skillName.text = currentCharacterInfo.startingWeapon.GetLocalizedTitle();
        if(skillDescription != null)
            skillDescription.text = currentCharacterInfo.startingWeapon.GetCurrentDescription();
    }
    public void DeselectOtherButtons()
    {
        foreach (CharacterSelectionButton button in characterButtons)
        {
            button.DeSelected();
        }
    }
    public void Purchase()
    {
        if (PlayerStats.Instance.TrySpendGold(characterSelectionButton.characterInfo.costPerLevel))
        {
            characterSelectionButton.characterInfo.purchased = true;
            characterSelectionButton.CardIcon.color = Color.white;
            BuyButtons.SetActive(false);
        }
    }
    public void SaveCharacterData()
    {
        if (characterList == null || characterList.Count == 0)
        {
            Debug.LogError("Character list is empty or not assigned.");
            return;
        }

        List<CharacterSaveData> saveDataList = new List<CharacterSaveData>();

        // Populate the save data list
        foreach (var character in characterList)
        {
            if (character == null)
            {
                Debug.LogWarning("A CharacterScriptableObject in the list is null. Skipping.");
                continue;
            }

            saveDataList.Add(new CharacterSaveData
            {
                CharacterName = character.CharacterName,
                purchased = character.purchased
            });
        }

        // Convert the save data to JSON and write to file
        string json = JsonUtility.ToJson(new SaveDataWrapper<CharacterSaveData> { data = saveDataList }, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("Character data saved to " + saveFilePath);
    }

    /// <summary>
    /// Loads the character data from a JSON file.
    /// </summary>
    public void LoadCharacterData()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("Save file not found. No data to load.");
            return;
        }

        // Read JSON data from the file
        string json = File.ReadAllText(saveFilePath);
        var saveDataWrapper = JsonUtility.FromJson<SaveDataWrapper<CharacterSaveData>>(json);

        if (saveDataWrapper?.data == null)
        {
            Debug.LogError("Failed to load character data. Save file might be corrupted.");
            return;
        }

        // Update the ScriptableObjects based on the saved data
        foreach (var saveData in saveDataWrapper.data)
        {
            foreach (var character in characterList)
            {
                if (character != null && character.CharacterName == saveData.CharacterName)
                {
                    character.purchased = saveData.purchased;
                    Debug.Log($"Loaded {character.CharacterName}: purchased = {character.purchased}");
                }
            }
        }
    }

    public void ConfirmCharacter()
    {
        PlayerStats.Instance.CharacterID = characterSelectionButton.characterInfo.Id;
        //SaveLoadManager.Instance.SettingMode(1);//세 게임
    }

    public void ResetCharacters()
    {
        foreach(var character in characterList)
        {
            if (character.purchased == true)
                PlayerStats.Instance.AddGold(character.costPerLevel);

            character.purchased = false;
        }
        characterList[0].purchased = true;

        foreach (CharacterSelectionButton button in characterButtons)
        {
            button.Initialize(this);
        }
        characterButtons[0].CardIcon.color = Color.white;
        characterButtons[0].Selected();
        SaveCharacterData();
    }
    [System.Serializable]
    private class SaveDataWrapper<T>
    {
        public List<T> data;
    }
}
[System.Serializable]
public class CharacterSaveData
{
    public string CharacterName;
    public bool purchased;
}
