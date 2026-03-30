using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLever : MonoBehaviour
{
    [Header("기본 설정")]
    public int myIndex;       // 나의 번호 (0, 1, 2...)
    public bool IsOn = false; // 현재 상태

    [Header("규칙 설정 (중요!)")]
    [Tooltip("이 레버를 당겼을 때 같이 움직일 다른 레버들의 번호")]
    public List<int> affectedIndices; 

    [Header("참조")]
    //public Sprite onSprite;
    //public Sprite offSprite;
    public LeverPuzzleManager manager;
    private SpriteRenderer sr;
    public string ProgressID;//한번깨면 계속 On상태로 유지하도록

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        CheakInit();
    }
    public void CheakInit()
    {
        if (GameProgressManager.Instance.IsUnlocked(ProgressID))
        {
            IsOn = true;
            UpdateVisual();
        }
        else
        {
            ResetToggle();
        }
    }

    public void Interact()
    {
        // 매니저에게 "나 당겨졌는데, 내 규칙(affectedIndices)대로 처리해줘"라고 요청
        manager.OnLeverPulled(myIndex, affectedIndices);
    }

    // 매니저가 시켜서 상태를 바꿀 때
    public void Toggle()
    {
        IsOn = !IsOn;
        UpdateVisual();
    }
    public void ResetToggle()
    {
        IsOn = false;
        UpdateVisual();
    }
    void UpdateVisual()
    {
        if(sr != null) sr.flipX = IsOn ? false : true;
    }
}
