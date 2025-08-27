using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CharacterSelectionManager : MonoBehaviour
{
    public List<CharacterSelectionButton> characterButtons = new List<CharacterSelectionButton>();
  
    public GameObject panle;
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public TMP_Text playerStatsText;
    public GameObject BuyButtons;
    public GameObject ConfirmButton;
    public Image icon;
    public CharacterSelectionButton characterSelectionButton;

    [SerializeField] private List<CharacterScriptableObject> characterList; // List of CharacterScriptableObjects
    private string saveFilePath;

    private void Awake()
    {
        // Set the save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, "CharacterData.json");
        LoadCharacterData();
    }
    private void Start()
    {
        foreach(CharacterSelectionButton button in characterButtons)
        {
            button.Initialize(this);
        }
        Invoke("delayedStart", .5f);
        
    }
    void delayedStart()
    {
        characterButtons[0].Selected();
    }
    public void SetInfo(CharacterScriptableObject info,CharacterSelectionButton button)
    {
        characterSelectionButton = button;
        panle.SetActive(true);
        nameText.text = info.CharacterName;
        descriptionText.text = info.description;
        costText.text = info.costPerLevel.ToString();
        icon.sprite = info.IconSprite;

        string playerStats = $"<b>Stats:</b>\n" +
               $"Base HP: {info.BaseHP}\n" +
               $"Damage: {info.Damage}\n" +
               $"Movement Speed: {info.MovementSpeed}\n" +
               $"Armor: {info.Armor}\n" +
               $"Health Regeneration: {info.HealthRegeneration}\n" +
               $"Luck Boost: {info.LuckBoost}%\n" +
               $"Cooldown Reduction: {info.CooldownReduction}%\n" +
               $"Double Damage Chance: {info.DobleDamageChance}%\n" +
               $"XP Boost: {info.XPBoost}%";

        playerStatsText.text = playerStats;
        BuyButtons.SetActive(!info.purchased);
        ConfirmButton.SetActive(info.purchased);
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
        if (PlayerStats.Instance.GoldAmount >= characterSelectionButton.characterInfo.costPerLevel )
        {
            PlayerStats.Instance.GoldAmount -= characterSelectionButton.characterInfo.costPerLevel;
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
    }

    public void ResetCharacters()
    {
        foreach(var character in characterList)
        {
            if (character.purchased == true)
                PlayerStats.Instance.GoldAmount += character.costPerLevel;
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
