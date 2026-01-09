using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    [Header("UI 연결")]
    public GameObject promptCanvas; // 아까 만든 InteractionCanvas 전체
    public TMPro.TextMeshProUGUI promptText;// 메시지 내용 (선택사항)

    private Interactor currentInteractable; // 현재 상호작용 가능한 대상

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // 시작할 땐 UI 숨김
        promptCanvas.SetActive(false);
    }

    private void Update()
    {
        // 상호작용 대상이 있고, E키를 눌렀을 때
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.TriggerInteraction();
        }
    }

    public void RegisterInteractable(Interactor interactable)
    {
        currentInteractable = interactable;
        
        // UI를 해당 오브젝트 위치로 이동시키고 켬
        //promptCanvas.transform.position = interactable.transform.position + uiOffset;
        promptCanvas.SetActive(true);

        if(promptText != null) promptText.text = interactable.actionName; // 예: "열기", "대화하기"
    }

    public void UnregisterInteractable(Interactor interactable)
    {
        if (currentInteractable == interactable)
        {
            currentInteractable = null;
            promptCanvas.SetActive(false);
        }
    }
}