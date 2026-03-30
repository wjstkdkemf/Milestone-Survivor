using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    public int MapLevel;
    public string SceneName; // 사용할 맵의 이름.
    public List<Wave> waves = new List<Wave>(); 
    public EnCounterSystem enCounterSystem;
    public bool BossEncounter = false;

    [Header("오디오 설정 (Audio Settings)")]
    [Tooltip("이 맵(지역)을 탐험할 때 나오는 기본 배경음악")]
    public AudioClip explorationBGM; 
    
    [Tooltip("인카운터(전투)가 발생했을 때 나오는 전투 배경음악")]
    public AudioClip battleBGM;     

    void Start()
    {
        if (enCounterSystem == null)
        {
            enCounterSystem = EnCounterSystem.Instance;
            if (explorationBGM != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGMWithFade(explorationBGM);
            }
            if (enCounterSystem == null)
            {
                Debug.Log("인카운트 시스템 할당 실패");
            }
        }
        Debug.Log("맵 메이커 스타트");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && enCounterSystem != null && !BossEncounter)
        {
            enCounterSystem.EnterMap(this);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && enCounterSystem != null && !BossEncounter)
        {
            enCounterSystem.ExitMap();
        }
    }
}