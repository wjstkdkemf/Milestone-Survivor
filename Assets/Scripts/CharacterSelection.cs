using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class CharacterSelection : MonoBehaviour
{
    public GameObject Player;
   // public RuntimeAnimatorController[] characterAnimators; // Use RuntimeAnimatorController
    public CharacterScriptableObject[] characterData;
    //public Sprite[] characterIcons;
    public Image characterIconImage;
    private PlayerStats playerStats;

    void Start()
    {
        // Get the selected character from GameManager
        //int selectedCharacter = PlayerStats.Instance.CharacterID;


        //Player.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = characterAnimators[selectedCharacter];
        //characterIconImage.sprite = characterIcons[selectedCharacter];
    }

    public void OnceSetting()
    {
        int selectedCharacter = PlayerStats.Instance.CharacterID;

        if (PlayerStatsCalculate.Instance != null)
        {
            PlayerStatsCalculate.Instance.SetBaseStats(
                characterData[selectedCharacter].BaseHP,
                characterData[selectedCharacter].MovementSpeed,
                characterData[selectedCharacter].HealthRegeneration,
                characterData[selectedCharacter].XPBoost,
                characterData[selectedCharacter].LuckBoost,
                characterData[selectedCharacter].Damage,
                characterData[selectedCharacter].statModifiers
            );
        }
        else
            Debug.Log("PlayerStatsCalculate가 존재하지않음");

        Player.GetComponent<PlayerHealth>().UpdateHealthUI();

        CharacterScriptableObject characterDatas = characterData[PlayerStats.Instance.CharacterID];
        characterIconImage.sprite = characterDatas.IconSprite;

        // 애니메이터 적용
        if (characterDatas.animatorController != null) {
            Player.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = characterDatas.animatorController;
        }

        if (characterDatas.startingWeapon != null) {
        // 플레이어의 무기 관리자(PlayerWeaponController)를 찾아 무기 등록
            Player.GetComponent<PlayerWeaponController>().AddWeapon(characterDatas.startingWeapon);
            Player.GetComponent<PlayerWeaponController>().ToggleCombatMode(false);
        }

        // switch (selectedCharacter)
        // {
        //     case 0:
        //         UpgradeManager.Instance.ShootProjectile();
        //         break;
        //     case 1:
        //         UpgradeManager.Instance.KnifeProjectile();
        //         break;
        //     case 2:
        //         UpgradeManager.Instance.LightningBolt();
        //         break;
        //     case 3:
        //         UpgradeManager.Instance.SwordSlash();
        //         break;
        // }
        Debug.Log("초기화 설정");
        //UpgradeManager.Instance.SaveUpgrade();
    }
}