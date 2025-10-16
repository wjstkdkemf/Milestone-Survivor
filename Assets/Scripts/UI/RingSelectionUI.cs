using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InventorySystem;

public class RingSelectionUI : MonoBehaviour
{
    public static RingSelectionUI Instance { get; private set; }

    [SerializeField]
    private GameObject selectionPanel; // 슬롯 선택 버튼들이 위치할 패널
    [SerializeField]
    private Transform buttonContainer; // 버튼들이 실제로 생성될 부모 컨테이너
    [SerializeField]
    private GameObject buttonPrefab; // 선택 버튼 프리팹

    [Tooltip("여기에 고정된 링 슬롯 이름들을 입력하세요. (예: Ring1, Ring2)")]
    [SerializeField]
    private List<string> ringSlotNames = new List<string> { "Ring1", "Ring2" };

    private InventoryItem currentRing; // 장착 대기중인 반지 아이템

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        selectionPanel.SetActive(false); // 처음에는 UI를 숨김
    }

    public void ShowSelection(InventoryItem ringItem, Vector3 position)
    {
        if (ringItem.GetEquit() == true) return;
        
        currentRing = ringItem;

        // 슬롯 위치(position)에서 오른쪽으로 150px 떨어진 곳에 패널을 표시합니다.
        buttonContainer.transform.position = position + new Vector3(150, 0, 0);

        // 기존 버튼들 삭제
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // 고정된 링 슬롯만큼 버튼 생성
        foreach (string slotType in ringSlotNames)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = slotType; // 버튼 텍스트 설정 (e.g., "Ring1")
            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnSlotSelected(slotType));
        }

        // 패널을 하이어라키의 맨 마지막으로 보내 가장 앞에 보이게 함
        selectionPanel.transform.SetAsLastSibling();
        selectionPanel.SetActive(true);
    }

    private void OnSlotSelected(string slotType)
    {
        // 인벤토리 컨트롤러에 특정 슬롯에 아이템 장착을 요청
        InventoryController.instance.EquipRingInSlot(currentRing, slotType);
        selectionPanel.SetActive(false); // 패널 숨기기
    }

    public void HideSelection()
    {
        if (selectionPanel.activeSelf)
        {
            selectionPanel.SetActive(false);
        }
    }
}
