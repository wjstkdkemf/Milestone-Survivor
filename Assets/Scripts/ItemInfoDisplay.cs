using UnityEngine;
using UnityEngine.UI;
using InventorySystem;

public class ItemInfoDisplay : MonoBehaviour
{
    [Tooltip("아이템 정보를 표시할 UI Text 컴포넌트")]
    public TMPro.TextMeshProUGUI infoText; // TextMeshPro를 사용한다면 public TMPro.TextMeshProUGUI infoText; 로 변경
    //public Sprite itemSprite;
    public Image itemImage;


    [Tooltip("정보를 표시할 대상 슬롯 타입")]
    public string targetSlotType = "Inventory"; // 원하는 slotType으로 변경하세요.

    void OnEnable()
    {
        // InventoryEventSystem의 OnSlotClicked 이벤트에 구독(리스너 등록)
        InventoryEventSystem.OnSlotClicked += HandleSlotClick;
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독 해제
        InventoryEventSystem.OnSlotClicked -= HandleSlotClick;
    }

    private void HandleSlotClick(InventoryItem item, string slotType)
    {
        // 클릭된 슬롯의 타입이 내가 원하는 타입과 일치하는지 확인
        if (slotType == targetSlotType)
        {
            itemImage.sprite = item.GetItemImage();

            if (infoText != null)
            {
                // 아이템 정보를 Text UI에 표시
                infoText.text = $"아이템 이름: {item.GetItemType()}\n" +
                                $"수량: {item.GetAmount()}\n" +
                                $"강화 레벨: {item.GetEnhancementLevel()}";

                // Inform the ItemEnchanter about the selected item
                if (ItemEnchanter.Instance != null)
                {
                    ItemEnchanter.Instance.SetItem(item);
                }
            }
        }
    }
}
