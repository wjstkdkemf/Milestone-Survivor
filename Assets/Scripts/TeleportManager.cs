
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;
    public string startPointName = "StartPoint";
    public string startMapName = "StartMap";

    private List<TeleportPoint> teleportPoints;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            teleportPoints = new List<TeleportPoint>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void RegisterPoint(TeleportPoint point)
    {
        teleportPoints.Add(point);
    }
    public void UnRegisterPoint(TeleportPoint point)
    {
        teleportPoints.Remove(point);
    }
    public void ResetPoints()
    {
        teleportPoints.Clear();
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

    public void SetInitialSpawnPoint()//GameObject player
    {
        MainMapManager.Instance.ChangeMap(startMapName, startPointName);
        //TeleportPlayer(player, startPointName);
    }

    public List<TeleportPoint> GetTeleportPoints()
    {
        return teleportPoints;
    }

    public void SetName(string Mapname, string pointName)//버튼에 넣을것
    {
        startMapName = Mapname;
        startPointName = pointName;
    }
}
