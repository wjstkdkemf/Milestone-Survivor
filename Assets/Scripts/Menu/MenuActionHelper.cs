using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public class MenuActionHelper : MonoBehaviour
{
    public enum ActionType 
    { 
        Open,           // 새 창 열기
        CloseCurrent,   // 현재 창 닫기 
        CloseAll,       // 전부 다 닫기
        Toggle          // 껐다 켜기
    }

    public ActionType actionType;
    public GameObject targetMenu;
    private void Awake()
    {
        Button myButton = GetComponent<Button>();
        
        if (myButton != null)
        {
            myButton.onClick.AddListener(ExecuteAction);
        }
    }

    public void ExecuteAction()
    {
        if (MenuButtonController.Instance == null)
        {
            Debug.LogError("MenuButtonController가 씬에 없습니다!");
            return;
        }

        switch (actionType)
        {
            case ActionType.Open:
                if (targetMenu != null) MenuButtonController.Instance.OpenMenu(targetMenu);
                break;
            case ActionType.CloseCurrent:
                MenuButtonController.Instance.CloseCurrentMenu();
                break;
            case ActionType.CloseAll:
                MenuButtonController.Instance.CloseAllMenus();
                break;
            case ActionType.Toggle:
                if (targetMenu != null) MenuButtonController.Instance.ToggleMenu(targetMenu);
                break;
        }
    }
}