
using UnityEngine;

public class Teleporter : MonoBehaviour
{
     // Default start point

    void Start()
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.SetInitialSpawnPoint();//gameObject
        }
    }

    public void TeleportTo(string teleportMapName, string teleportPointName)
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.TeleportPlayer(gameObject, teleportMapName ,teleportPointName);
        }
    }
}
