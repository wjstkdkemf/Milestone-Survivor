using UnityEngine;
using InventorySystem;

public class TrashHandler : MonoBehaviour
{
    // ItemData ScriptableObject들이 저장된 경로 (Resources 폴더 하위)
    private const string ITEM_DATA_PATH = "Items";

    public void TrashItem(InventoryItem item)
    {
        if (item == null || item.GetIsNull())
        {
            return;
        }

        // 아이템 이름으로 ItemData를 로드합니다.
        // ScriptableObject는 Resources.Load를 통해 불러올 수 있어야 합니다.
        //ItemData data = Resources.Load<ItemData>($"{ITEM_DATA_PATH}/{item.GetItemType()}");
        
        //if (data != null)
        //{
        //    PlayerStats의 골드를 아이템 가격만큼 증가시킵니다.
        //    PlayerStats.Instance.AddCoin(data.price);
        int slotIndex = item.GetPosition();
        PlayerStats.Instance.AddCoin(item.GetPrice());

        
        //}
        //else
        //{
        //    Debug.LogWarning($"[TrashHandler] '{item.GetItemType()}'에 대한 ItemData를 찾을 수 없습니다. 골드가 추가되지 않습니다.");
        //}

        // 인벤토리에서 아이템을 제거합니다.
        InventoryController.instance.RemoveItem(item.GetInventory(), item, item.GetAmount());
        Debug.Log($"[TrashHandler] '{item.GetItemType()}' 아이템을 팔았습니다.");
    }
}
