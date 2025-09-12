using UnityEngine;
using InventorySystem;

public class SceneInventoryLoader : MonoBehaviour
{
    void Start()
    {
        // 씬에 있는 InventoryController 인스턴스를 찾습니다.
        InventoryController controller = InventoryController.instance;

        if (controller != null)
        {
            // 런타임에 인벤토리 UI를 초기화(생성)합니다.
            controller.InitializeInventories();
            Debug.Log("SceneInventoryLoader: 런타임에 인벤토리 초기화를 완료했습니다.");
        }
        else
        {
            Debug.LogError("SceneInventoryLoader: 씬에서 InventoryController를 찾을 수 없습니다!");
        }
    }
}
