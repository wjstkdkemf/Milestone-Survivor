using UnityEngine;
using UnityEngine.UI;

public class WeaponIconUI : MonoBehaviour
{
// 유니티 인스펙터 창에서 자식 오브젝트(스킬 아이콘 Image)를 이 칸에 드래그 앤 드롭 해둡니다!
    [Header("자식 아이콘 이미지 연결")]
    public Image targetIconImage; 

    // 외부에서 아이콘을 세팅하라고 명령할 때 쓰는 함수
    public void SetIcon(Sprite newIcon)
    {
        if (targetIconImage != null)
        {
            targetIconImage.sprite = newIcon;
        }
    }
}
