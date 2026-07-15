using System.Collections.Generic;
using UnityEngine;

public class MenuButtonController : MonoBehaviour
{
    public static MenuButtonController Instance { get; private set; }
    private List<GameObject> menuList = new List<GameObject>(); //Stack으로 활용
	public PanelMessage warningPanel;
    private PlayerInputReader playerInputReader;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        playerInputReader = FindObjectOfType<PlayerInputReader>();
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.I)) ToggleMenu(inventoryPanel);
        if (playerInputReader != null && playerInputReader.CancelPressed) HandleEscapeKey();
    }

    public void OpenMenu(GameObject menuToOpen)
    {
        if (menuList.Count > 0 && menuList[menuList.Count - 1] == menuToOpen) return;

        if (menuList.Count > 0)
        {
            menuList[menuList.Count - 1].SetActive(false);
        }

        if (menuList.Contains(menuToOpen))
        {
            menuList.Remove(menuToOpen);
        }

        menuList.Add(menuToOpen);
        menuToOpen.SetActive(true);

        Time.timeScale = 0f; 
    }

    public void ToggleMenu(GameObject menuToToggle)
    {
        if (menuList.Count > 0 && menuList[menuList.Count - 1] == menuToToggle)
        {
            CloseCurrentMenu();
        }
        else
        {
            OpenMenu(menuToToggle);
        }
    }

    public void CloseCurrentMenu()
    {        
        if (menuList.Count == 0) return;

        int lastIndex = menuList.Count - 1;
        GameObject topMenu = menuList[lastIndex];
        topMenu.SetActive(false);
        menuList.RemoveAt(lastIndex);

        if (menuList.Count > 0)
        {
            menuList[menuList.Count - 1].SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void CloseAllMenus()
    {
        foreach (var menu in menuList)
        {
            menu.SetActive(false);
        }
        menuList.Clear();
        Time.timeScale = 1f;
    }

    private void HandleEscapeKey()
    {
        if (menuList.Count > 0) CloseCurrentMenu();
        //else if (pauseMenuPanel != null) OpenMenu(pauseMenuPanel);
    }
	public void ScreenMessage(string message)
    {
        if (warningPanel != null)
        {
            warningPanel.ShowMessage(message, 5.0f);
        }
    }
}