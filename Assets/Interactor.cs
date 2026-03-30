using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactor : MonoBehaviour
{
    [Header("설정")]
    public string actionName = "상호작용"; // UI에 띄울 텍스트 (예: 열기, 줍기)
    
    [Header("실행할 기능")]
    public UnityEvent OnInteract; // 인스펙터에서 드래그 앤 드롭으로 연결 가능!

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 매니저에게 나를 등록 (UI 띄워줘!)
            InteractionManager.Instance.RegisterInteractable(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 매니저에게 등록 해제 (UI 꺼줘!)
            InteractionManager.Instance.UnregisterInteractable(this);
        }
    }

    // 실제로 E키를 눌렀을 때 실행될 함수
    public void TriggerInteraction()
    {
        Debug.Log($"{gameObject.name}와 상호작용했습니다.");
        OnInteract?.Invoke(); // 연결된 이벤트들 실행
    }
}
