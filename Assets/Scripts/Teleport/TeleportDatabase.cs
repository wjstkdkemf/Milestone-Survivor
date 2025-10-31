using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeleportDatabase", menuName = "Teleport/Database")]
public class TeleportDatabase : ScriptableObject
{
    public List<TeleportZoneData> allZoneGroups;
}
