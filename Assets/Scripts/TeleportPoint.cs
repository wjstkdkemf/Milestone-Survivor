
using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    public string teleportPointName;

    void OnEnable()
    {
        if (TeleportManager.Instance != null)
        {
            Debug.Log("포인트 만들어짐");
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
