using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public GameObject Next_Portal;

    [Tooltip("이 포탈을 탔을 때 로드할 맵 프리팹")]
    public string targetMapPrefab;

    [Tooltip("새 맵에서 플레이어가 생성될 위치")]
    public string targetSpawnPosition;

    public Vector3 Next_pos = new Vector3(0, 0, 0);

    private void OnTriggerEnter2D(Collider2D other)
    {
        MainMapManager.Instance.ChangeMap(targetMapPrefab, targetSpawnPosition);
        if (Next_Portal != null)
        {
            // Transform trans = other.gameObject.GetComponent<Transform>();
            // Debug.Log(Next_Portal.GetComponent<Portal>().Next_pos);
            // trans.position = Next_Portal.GetComponent<Portal>().Next_pos;
            
        }
    }
}
