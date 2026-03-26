using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    [Header("탭 내용물 패널들 (순서대로 넣으세요)")]
    public List<GameObject> tabContents;

    [Header("탭 버튼들 (선택 사항: 누른 버튼 색상 강조용)")]
    public List<Button> tabButtons;
    public Color selectedColor = Color.white;
    public Color unselectedColor = Color.gray;
    private void Awake()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            if (tabButtons[i] != null)
            {
                int index = i; 
                
                tabButtons[i].onClick.AddListener(() => SwitchTab(index));
            }
        }
    }
    private void OnEnable()
    {
        SwitchTab(0);
    }
    public void SwitchTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabContents.Count) return;

        for (int i = 0; i < tabContents.Count; i++)
        {
            if (tabContents[i] != null)
                tabContents[i].SetActive(false);

            if (tabButtons.Count > i && tabButtons[i] != null)
                tabButtons[i].image.color = unselectedColor;
        }

        if (tabContents[tabIndex] != null)
            tabContents[tabIndex].SetActive(true);

        if (tabButtons.Count > tabIndex && tabButtons[tabIndex] != null)
            tabButtons[tabIndex].image.color = selectedColor;
    }
}