using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    public GameObject Player;
    public RuntimeAnimatorController[] characterAnimators; // Use RuntimeAnimatorController
    public CharacterScriptableObject[] characterData;
    public Sprite[] characterIcons;
    public Image characterIconImage;
    private PlayerStats playerStats;

    void Start()
    {
        // Get the selected character from GameManager
        int selectedCharacter = PlayerStats.Instance.CharacterID;

        Debug.Log(selectedCharacter);

        Player.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = characterAnimators[selectedCharacter];
        characterIconImage.sprite = characterIcons[selectedCharacter];
    }

    public void OnceSetting()
    {
        int selectedCharacter = PlayerStats.Instance.CharacterID;

        Player.GetComponent<PlayerHealth>().MaxHealth = characterData[selectedCharacter].BaseHP;
        Player.GetComponent<Player_Controller>().movmentSpeed = characterData[selectedCharacter].MovementSpeed;
        PlayerStats.Instance.HealthRegeneration = characterData[selectedCharacter].HealthRegeneration;
        PlayerStats.Instance.experienceBonus += characterData[selectedCharacter].XPBoost;
        PlayerStats.Instance.CharacterID += selectedCharacter;
        PlayerStats.Instance.LuckBonus += characterData[selectedCharacter].LuckBoost;
        PlayerStats.Instance.DamageBonus += characterData[selectedCharacter].Damage;

        Debug.Log(selectedCharacter);

        switch (selectedCharacter)
        {
            case 0:
                UpgradeManager.Instance.ShootProjectile();
                break;
            case 1:
                UpgradeManager.Instance.KnifeProjectile();
                break;
            case 2:
                UpgradeManager.Instance.LightningBolt();
                break;
            case 3:
                UpgradeManager.Instance.SwordSlash();
                break;
        }
        Debug.Log("초기화 설정");
        UpgradeManager.Instance.SaveUpgrade();
    }
}