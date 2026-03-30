using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharcterInfoDisplay : MonoBehaviour
{
    [Tooltip("캐릭터 정보를 표시할 UI Text 컴포넌트")]
    public TMPro.TextMeshProUGUI infoText; // TextMeshPro를 사용한다면 public TMPro.TextMeshProUGUI infoText; 로 변경
    //public Sprite itemSprite;
    public Image itemImage;


    void OnEnable()
    {

    }

    void OnDisable()
    {
    }

        private void HandleSlotClick(CharacterScriptableObject Character, string slotType)
    {
    }
}
