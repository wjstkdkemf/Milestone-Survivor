
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public string startPointName = "StartPoint"; // Default start point

    void Start()
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.SetInitialSpawnPoint(gameObject, startPointName);
        }
    }

    public void TeleportTo(string teleportPointName)
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.TeleportPlayer(gameObject, teleportPointName);
        }
    }
}
