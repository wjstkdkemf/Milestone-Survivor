
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;

    private List<TeleportPoint> teleportPoints;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        teleportPoints = FindObjectsOfType<TeleportPoint>().ToList();
    }

    public void TeleportPlayer(GameObject player, string teleportPointName)
    {
        TeleportPoint destination = teleportPoints.FirstOrDefault(point => point.teleportPointName == teleportPointName);

        if (destination != null)
        {
            player.transform.position = destination.transform.position;
        }
        else
        {
            Debug.LogWarning("Teleport point not found: " + teleportPointName);
        }
    }

    public void SetInitialSpawnPoint(GameObject player, string spawnPointName)
    {
        TeleportPlayer(player, spawnPointName);
    }

    public List<TeleportPoint> GetTeleportPoints()
    {
        return teleportPoints;
    }
}
