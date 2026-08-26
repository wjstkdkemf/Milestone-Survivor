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
    [SerializeField]private SpriteRenderer weaponSprite;
    [SerializeField]private WeaponVisualController weaponVisual;
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
        if (!TryGetSelectedCharacter(out int selectedCharacter, out CharacterScriptableObject characterDatas))
        {
            return;
        }

        if (PlayerStatsCalculate.Instance != null)
        {
            //베이스 스탯 추가시 수정점
            PlayerStatsCalculate.Instance.SetBaseStats(
                characterDatas.BaseHP,
                characterDatas.MovementSpeed,
                characterDatas.HealthRegeneration,
                characterDatas.LuckBoost,
                characterDatas.Damage,
                characterDatas.statModifiers
            );
            PlayerStatsCalculate.Instance.LevelUpBonus(0);
        }
        else
            Debug.LogWarning("PlayerStatsCalculate가 존재하지않음");

        if (Player != null && Player.TryGetComponent<PlayerHealth>(out var playerHealth))
            playerHealth.UpdateHealthUI();

        if (characterIconImage != null)
            characterIconImage.sprite = characterDatas.IconSprite;

        // 애니메이터 적용
        if (characterDatas.animatorController != null && Player != null && Player.transform.childCount > 0)
        {
            Animator animator = Player.transform.GetChild(0).GetComponent<Animator>();
            if (animator != null)
                animator.runtimeAnimatorController = characterDatas.animatorController;
        }

        if (UpgradeManager.Instance != null) 
        {
            UpgradeManager.Instance.ResetRunData(characterDatas.StartingDeck); // 업글레이드 매니저 초기화.

            if (characterDatas.startingWeapon != null)
                UpgradeManager.Instance.OnUpgradeSelected(characterDatas.startingWeapon);
        }
        if(weaponVisual != null)
        {
            weaponVisual.SetBaseLocalPosition(characterDatas.weaponLocalPosition);
            weaponVisual.SetDirectionalOffset(characterDatas.weaponLocalDirection);
            weaponVisual.SetRotationOffset(characterDatas.weaponRotationOffset);
        
            if (weaponSprite != null)
                weaponSprite.sprite = characterDatas.WeaponSprite;
        }
        if(weaponVisual != null)
        {
            weaponVisual.mode = characterDatas.WeaponVisualMode;
        }

        if (characterDatas.startingWeapon != null && Player != null && Player.TryGetComponent<PlayerWeaponController>(out var weaponController)) {
        // 플레이어의 무기 관리자(PlayerWeaponController)를 찾아 무기 등록
            //Player.GetComponent<PlayerWeaponController>().AddWeapon(characterDatas.startingWeapon);
            weaponController.ToggleCombatMode(false);
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
        DevLog.Log("초기화 설정");
        //UpgradeManager.Instance.SaveUpgrade();
    }

    private bool TryGetSelectedCharacter(out int selectedCharacter, out CharacterScriptableObject character)
    {
        selectedCharacter = 0;
        character = null;

        if (PlayerStats.Instance == null)
        {
            Debug.LogError("[CharacterSelection] PlayerStats is missing.");
            return false;
        }

        if (characterData == null || characterData.Length == 0)
        {
            Debug.LogError("[CharacterSelection] Character data is empty.");
            return false;
        }

        selectedCharacter = Mathf.Clamp(PlayerStats.Instance.CharacterID, 0, characterData.Length - 1);
        character = characterData[selectedCharacter];

        if (character == null)
        {
            Debug.LogError($"[CharacterSelection] Character data at index {selectedCharacter} is missing.");
            return false;
        }

        if (selectedCharacter != PlayerStats.Instance.CharacterID)
        {
            Debug.LogWarning($"[CharacterSelection] CharacterID {PlayerStats.Instance.CharacterID} is out of range. Using {selectedCharacter} instead.");
            PlayerStats.Instance.CharacterID = selectedCharacter;
        }

        return true;
    }
}
