using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeUi : MonoBehaviour
{
    public UpgradeScriptableObject Upgrade;

    [SerializeField] private TMP_Text Title;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private Image Icon;
    [SerializeField] private string UpgradeName;
    [SerializeField] private TMP_Text UpgradeLevel;
    public List<GameObject> UpgradePointsList;
    // Start is called before the first frame update


    // Update is called once per frame
    void FixedUpdate()
    {

    }


    public void SetInfo(UpgradeScriptableObject info)
    {
        // foreach (GameObject go in UpgradePointsList)
        // {
        //     go.transform.GetChild(0).gameObject.SetActive(false);
        // }
Upgrade = info;

        // 1. 기본 정보 표시
        if (Title != null) Title.text = Upgrade.Title;
        if (Icon != null) Icon.sprite = Upgrade.Icon;
        
        // 2. 설명 표시
        // (팁: 만약 레벨별로 설명을 다르게 하고 싶다면 리스트에서 가져오게 수정 가능)
        if (Description != null) Description.text = Upgrade.Description;

        // 3. 레벨 표시 (현재 레벨 + 1 = "다음 레벨"을 보여줌)
        if (UpgradeLevel != null) 
        {
            UpgradeLevel.text = "Lv." + (Upgrade.Points + 1).ToString();
        }

    }
    public void UpgradeFunction()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeSelected(Upgrade);
        }

    }

    // void ClearEncount()
    // {
    //     GameObject Encounter = GameObject.FindWithTag("EnCount");
    //     Debug.Log("체크포인트 1");
    //     if (Encounter != null)
    //     {
    //         EnCounterSystem enCounterSystem = Encounter.GetComponent<EnCounterSystem>();
    //         if (enCounterSystem != null)
    //         {
    //             enCounterSystem.ClearEncount();
    //         }
    //     }
    // }
}
