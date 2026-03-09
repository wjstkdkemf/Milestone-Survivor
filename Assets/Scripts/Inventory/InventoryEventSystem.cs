
using System;
using InventorySystem;

public static class InventoryEventSystem
{
    // 슬롯이 클릭되었을 때 호출될 이벤트입니다.
    // 클릭된 슬롯의 InventoryItem과 slotType을 전달합니다.
    public static event Action<InventoryItem, string> OnSlotClicked;

    // 이벤트를 발생시키는 함수입니다.
    public static void RaiseSlotClicked(InventoryItem item, string slotType)
    {
        OnSlotClicked?.Invoke(item, slotType);
    }
}
