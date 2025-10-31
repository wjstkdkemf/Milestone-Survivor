
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class TeleportUI : MonoBehaviour
{
    public GameObject teleportSmallButtonPrefab;
    public GameObject teleportBigButtonPrefab;

    public Transform smallButtonContainer;
    public Transform bigButtonContainer;

    public GameObject player;
    public bool IsHome;
    private string SelectGroup;
    private List<TeleportPoint> teleportPoints;

    void Start()
    {
        teleportPoints = TeleportManager.Instance.GetTeleportPoints();
        CreateBigTeleportButton();
        //CreateTeleportButtons();
        //gameObject.SetActive(false); // Initially hidden
    }
    void CreateBigTeleportButton()
    {
        // 기존 버튼 삭제
        foreach (Transform child in bigButtonContainer) Destroy(child.gameObject);

        // 데이터베이스에서 모든 '그룹'을 가져옴
        List<TeleportZoneData> zoneGroups = TeleportManager.Instance.database.allZoneGroups;


        foreach (TeleportZoneData group in zoneGroups)
        {

            GameObject buttonObj = Instantiate(teleportBigButtonPrefab, bigButtonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = group.zoneName; // 혹은 Text

            // (중요) 버튼 클릭 시 'PopulatePointList' 함수를 호출하도록 연결
            // 루프 안에서 리스너를 추가할 땐 변수를 복사해야 함
            TeleportZoneData currentGroup = group; 
            buttonObj.GetComponent<Button>().onClick.AddListener(() => 
            {
                CreateTeleportButtons(currentGroup);
            });
        }
    }

    void CreateTeleportButtons(TeleportZoneData selectedGroup)
    {
        // 기존 버튼 삭제
        ClearPointList();
        //SelectGroup = selectedGroup.zoneName;

        // 선택된 그룹에 속한 '포인트 데이터' 목록을 가져옴
        foreach (TeleportData pointData in selectedGroup.pointsInZone)
        {
            GameObject buttonObj = Instantiate(teleportSmallButtonPrefab, smallButtonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = pointData.displayName;

            // TeleportManager에서 이 포인트의 '잠금 해제' 여부를 물어봄
            bool unlocked = TeleportManager.Instance.IsPointUnlocked(pointData.targetSpawnPointID);

            Button pointButton = buttonObj.GetComponent<Button>();
            pointButton.interactable = unlocked; // 잠금 해제 안됐으면 비활성화

            if (unlocked)
            {
                // 잠금 해제되었다면 텔레포트 기능 연결
                TeleportData currentPoint = pointData;
                pointButton.onClick.AddListener(() =>
                {
                    OnTeleportButtonClick(currentPoint);
                });
            }
        }
    }
    private void ClearPointList()
    {
        foreach (Transform child in smallButtonContainer) Destroy(child.gameObject);
    }

    void OnTeleportButtonClick(TeleportData teleportPoint)//Name
    {
        if (IsHome)
        {
            TeleportManager.Instance.startMapName = teleportPoint.pointID;
            TeleportManager.Instance.startPointName = teleportPoint.targetSpawnPointID;
        }
        else if (player != null)
        {
            Teleporter teleporter = player.GetComponent<Teleporter>();
            if (teleporter != null)
            {
                teleporter.TeleportTo(teleportPoint.pointID, teleportPoint.targetSpawnPointID);
            }
            gameObject.SetActive(false); // Hide UI after teleporting
        }
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}
