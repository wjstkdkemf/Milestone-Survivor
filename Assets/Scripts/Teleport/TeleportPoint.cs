
using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    public string teleportPointName;
    public string teleportMapName;


    void OnEnable()
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.RegisterPoint(this);
        }
    }

    void OnDisable()
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.UnRegisterPoint(this);
        }
    }

}
