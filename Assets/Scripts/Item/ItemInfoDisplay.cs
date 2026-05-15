using UnityEngine;
using UnityEngine.UI;
using InventorySystem;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;
using System.Threading.Tasks;


public class ItemInfoDisplay : MonoBehaviour
{
    [Tooltip("아이템 정보를 표시할 UI Text 컴포넌트")]
    //public TMPro.TextMeshProUGUI infoText; // TextMeshPro를 사용한다면 public TMPro.TextMeshProUGUI infoText; 로 변경
    public LocalizeStringEvent[] infoTextEvent;

    //public Sprite itemSprite;
    public Image itemImage;


    [Tooltip("정보를 표시할 대상 슬롯 타입")]
    public string targetSlotType = "Inventory"; // 원하는 slotType으로 변경하세요.
    private LocalizedString localizedDefaultName;
    void Awake()
    {
        UpdateWithDefaultValues();
    }

    void UpdateWithDefaultValues()
    {
        infoTextEvent[0].StringReference.Arguments = new object[] {
            "---", // {0}
        };
        infoTextEvent[1].StringReference.Arguments = new object[] 
        {
            "일반",         // -> {0} (아이템 등급)
        };
        infoTextEvent[2].StringReference.Arguments = new object[] 
        {
            "아이템 설명이 없습니다.",         // -> {0} (아이템 설명)
        };

        Refresh();
    }
    void Refresh()
    {
        foreach(var eve in infoTextEvent)
        {
            eve.RefreshString();
        }
    }

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

    private async void HandleSlotClick(InventoryItem item, string slotType)
    {
        // 클릭된 슬롯의 타입이 내가 원하는 타입과 일치하는지 확인
        //if (slotType != targetSlotType)
            //return;
        
        if (item == null || item.GetIsNull() || item.GetAmount() <= 0)
        {
            itemImage.sprite = null; // (또는 기본 이미지)
            UpdateWithDefaultValues(); // 기본값("---", 0, 0)으로 UI 초기화
            
            if (ItemEnchanter.Instance != null)
            {
                ItemEnchanter.Instance.SetItem(null);
            }
            return; // 함수 종료
        }

        itemImage.sprite = item.GetItemImage();

        LocalizedString localizedItemNameRef = new LocalizedString();
        localizedItemNameRef.TableReference = "Item_Name_Table"; // 아이템 이름이 있는 테이블
        localizedItemNameRef.TableEntryReference = item.GetItemType(); // "Item.Name.HealthPotion"

        string translatedItemName = "Loading..."; // 임시 값
        
        try
        {
            var loadHandle = localizedItemNameRef.GetLocalizedStringAsync();
            translatedItemName = await loadHandle.Task; // 로드될 때까지 대기
        }
        catch (System.Exception e)
        {
            Debug.LogError($"아이템 이름 번역 실패 (Key: {item.GetItemType()}): {e.Message}");
            translatedItemName = item.GetItemType(); // 실패 시 Key 이름이라도 표시
        }

        infoTextEvent[0].StringReference.Arguments = new object[] 
        {
            translatedItemName,         // -> {0} (아이템 이름)
        };
        infoTextEvent[1].StringReference.Arguments = new object[] 
        {
            item.GetGrade(),         // -> {0} (아이템 등급)
        };
        infoTextEvent[2].StringReference.Arguments = new object[] 
        {
            item.GetDescription(),         // -> {0} (아이템 설명)
        };

        if (ItemEnchanter.Instance != null)
        {
            ItemEnchanter.Instance.SetItem(item);
        }

        Refresh();
        // if (infoText != null)
        // {
        //     // 아이템 정보를 Text UI에 표시
        //     infoText.text = $"아이템 이름: {item.GetItemType()}\n" +
        //                     $"수량: {item.GetAmount()}\n" +
        //                     $"강화 레벨: {item.GetEnhancementLevel()}";
        // }
        // Inform the ItemEnchanter about the selected item

        
    }
}
