
using UnityEngine;

public class Teleporter : MonoBehaviour
{
     // Default start point
    public bool isTeleporting = false;
    void Start()
    {
        if (TeleportManager.Instance != null)
        {
            //TeleportManager.Instance.SetInitialSpawnPoint();//gameObject
        }
    }

    public void TeleportTo(string teleportMapName, string teleportPointName)
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.TeleportPlayer(gameObject, teleportMapName ,teleportPointName);
        }
    }

    public void SetFloorInfo(int floorIndex, string layerName,string sortingName ,int sortingOrder)
    {
        // 레이어 변경
        this.gameObject.layer = LayerMask.NameToLayer(layerName);
        
        // 오더 변경
        var sr = GetComponentInChildren<SpriteRenderer>();
        if(sr != null) 
        {
            sr.sortingLayerName = sortingName;
            sr.sortingOrder = sortingOrder;
        }

        // 미니맵 변경
        MinimapController.Instance.ChangeFloor(floorIndex);
    }
}
