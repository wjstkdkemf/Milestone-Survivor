using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set;}
    public Transform player;
    public Camera minimapCamera;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.z = -10f; 
            minimapCamera.transform.position = newPosition;
        }
    }

    public void ChangeFloor(int floorIndex)
    {
        // 사장님이 완벽하게 짜두신 기존 코드 그대로 사용!
        int layerMask = 1 << LayerMask.NameToLayer("MiniMap_Common");
        string targetLayerName = "MiniMap_F" + floorIndex;
        layerMask |= 1 << LayerMask.NameToLayer(targetLayerName);

        minimapCamera.cullingMask = layerMask;
    }
}
