using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    public int MapLevel;
    public string SceneName;
    public EnCounterSystem enCounterSystem;

    void Start()
    {
        if (enCounterSystem == null)
        {
            enCounterSystem = EnCounterSystem.Instance;
            if (enCounterSystem == null)
            {
                Debug.Log("인카운트 시스템 할당 실패");
            }
        }
        Debug.Log("맵 메이커 스타트");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && enCounterSystem != null)
        {
            enCounterSystem.EnterMap(this);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && enCounterSystem != null)
        {
            enCounterSystem.ExitMap();
        }
    }
}
