using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    public PolygonCollider2D MapBounds;
    void Start()
    {
        ArenaManager.Instance.ChangeMap(MapBounds);
    }
}
