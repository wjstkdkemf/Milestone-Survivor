using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
public class MenuButtonController : MonoBehaviour
{
	public static MenuButtonController Instance;

	public GameObject MaineMenuObject;
	public GameObject CharacterSelectionObject;
	public GameObject PowerUpObject;
	public GameObject PauseObject;
	public GameObject SettingsObject;
	public GameObject AudioObject;
	public GameObject GraphicsObject;
	public GameObject UpgradeObject;
	public GameObject SelectObject;

	public GameObject LoadObject;
	public GameObject InventoryObject;
	public GameObject TeleportObject;
	public GameObject LanguageObject;

    public PanelMessage warningPanel;

	public bool Menu, Pause, Settings, SettingsTwo,SettingsThree,Settingsfour , CharacterSelection, PowerUp, Load , Save , Inventory , Teleport, Language;
	public GameObject CurntButton;

	private GameObject MaineMenuButton;
	private GameObject PauseButton;
	private GameObject SelectButton;

	private GameObject SettingsButton;
	private GameObject AudioButton;
	private GameObject GraphicsButton;
	public GameObject PowerUpButton;
	public GameObject CharacterSelectionButton;
	public string FunctionName;
	public bool ingame = false;
	public bool InGameUpgrade = false;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one MenuButtonController in scene");
		}
		else
		{
			Instance = this;
		}
	}
	private void Start()
	{
		MaineMenuButton = MaineMenuObject.transform.GetChild(0).gameObject;
		PauseButton = PauseObject.transform.GetChild(0).gameObject;
		SettingsButton = SettingsObject.transform.GetChild(0).gameObject;
		AudioButton = AudioObject.transform.GetChild(0).gameObject;
		GraphicsButton = GraphicsObject.transform.GetChild(0).gameObject;

		if (Menu)
		{
			EventSystem.current.SetSelectedGameObject(MaineMenuButton);

		}

	}
	// Update is called once per frame
	void Update()
	{
		if (EventSystem.current.currentSelectedGameObject == null)
		{
			EventSystem.current.SetSelectedGameObject(CurntButton);
		}

		if (Pause)
		{
			GameManager.Instance.Pause = true;
			GameManager.Instance.StopMoveing = true;
			UpgradeObject.SetActive(false);


		}
		// else if(Inventory && ingame)
        // {
        //     GameManager.Instance.Pause = true;
		// 	GameManager.Instance.StopMoveing = true;
        // }
		else if (!Pause && !UpgradeObject.activeSelf && UpgradeObject != null && !Menu)
		{
			GameManager.Instance.Pause = false;
			GameManager.Instance.StopMoveing = false;


		}


		if (Input.GetKeyDown(KeyCode.Escape))
		{
			back();
		}

		if (Input.GetKeyDown(KeyCode.I))
		{
			InventoryButton();
		}

	}


	public void Quit()
	{
		if (Menu == false)
		{

			SceneManager.LoadScene(0);

		}
		else
		{
			Application.Quit();
		}
	}

	public void ScreenMessage(string message)
    {
        if (warningPanel != null)
        {
            warningPanel.ShowMessage(message, 5.0f);
        }
    }

	public void ClearMap()
	{
		SceneManager.LoadScene("Village");
	}


	public void Escape()
	{
		CurntButton = PauseButton;
		EventSystem.current.SetSelectedGameObject(CurntButton);
		MaineMenuObject.SetActive(false);
		Pause = true;
		PauseObject.SetActive(true);

	}

	public void Setting()
	{

		CurntButton = SettingsButton;
		EventSystem.current.SetSelectedGameObject(CurntButton);
		SettingsObject.SetActive(true);
		MaineMenuObject.SetActive(false);
		PauseObject.SetActive(false);
		Settings = true;
	}
	public void SettingThree()
	{
		if (!SettingsThree)
		{
			CurntButton = SelectButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			SettingsThree = true;
			SelectObject.SetActive(true);
		}
		else
			back();
	}

	public void Audio()
	{
		CurntButton = AudioButton;
		EventSystem.current.SetSelectedGameObject(CurntButton);
		AudioObject.SetActive(true);
		SettingsObject.SetActive(false);
		SettingsTwo = true;
	}
	public void MainAudio()
    {
		AudioObject.SetActive(true);
		SelectObject.SetActive(false);
		SettingsTwo = true;
    }

	public void Graphics()
	{
		CurntButton = GraphicsButton;
		EventSystem.current.SetSelectedGameObject(CurntButton);
		GraphicsObject.SetActive(true);
		SettingsObject.SetActive(false);
		SettingsTwo = true;
	}
	public void MainGraghics()
    {
		GraphicsObject.SetActive(true);
		SelectObject.SetActive(false);
		SettingsTwo = true;
    }
	public void MainLanguage()
    {
		LanguageObject.SetActive(true);
		SelectObject.SetActive(false);
		Language = true;
    }
	public void PowerUps()
	{
		//CurntButton = PowerUpButton;
		//EventSystem.current.SetSelectedGameObject(CurntButton);
		PowerUp = true;
		PowerUpObject.SetActive(true);
		SelectObject.SetActive(false);
	}
	public void TeleportMap()
	{
		//CurntButton = PowerUpButton;
		//EventSystem.current.SetSelectedGameObject(CurntButton);
		Teleport = true;
		TeleportObject.SetActive(true);
		CharacterSelectionObject.SetActive(false);
		// if (TeleportObject.activeSelf)
		// {
		// 	Teleport = true;
		// 	TeleportObject.SetActive(false);
		// 	SelectObject.SetActive(true);
		// }
		// else
		// {
		// 	Teleport = false;
		// 	TeleportObject.SetActive(true);
		// 	SelectObject.SetActive(false);
		// }
	}
	public void CharacterSelect()
	{
		CharacterSelection = true;
		CharacterSelectionObject.SetActive(true);
		SelectObject.SetActive(false);
	}

	public void LoadGame()
	{
		CurntButton = MaineMenuButton;
		EventSystem.current.SetSelectedGameObject(CurntButton);
		Load = true;
		MaineMenuObject.SetActive(false);
		LoadObject.SetActive(true);
	}
	public void SaveGame()
	{
		// Save = true;
		// PauseObject.SetActive(false);
		// LoadObject.SetActive(true);
		Debug.Log("세이브버튼 입력");
		LoadScreenManager.Instance.ConfirmSelectionSave();
		Debug.Log("세이브완료");
	}

	public void back()
	{
		//==Debug.Log(Settings + " " +  SettingsTwo + " " + !Menu +" "+ !PowerUp +" "+ !CharacterSelection);
		// 인게임에서 '오디오' 또는 '그래픽' 설정 화면에 있을 때, '설정' 메뉴로 돌아갑니다.
		if (InGameUpgrade)
		{

		}
		else if (Settings && SettingsTwo && !Menu && !PowerUp && !CharacterSelection)//
		{
			CurntButton = SettingsButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			AudioObject.SetActive(false);
			GraphicsObject.SetActive(false);
			SettingsObject.SetActive(true);
			//SelectObject.SetActive(true);
			SettingsTwo = false;
		}
		//인게임에서 인벤토리창을 닫을때
		else if (ingame && Inventory)
		{
			Inventory = false;
			InventoryObject.SetActive(false);
			PlayerStatsCalculate.Instance.UpdatePlayerStats();
		}
		//메인화면에서 인벤토리 종료시
		else if (!Settings && !SettingsTwo && Inventory && !PowerUp && !CharacterSelection)
		{
			Inventory = false;
			InventoryObject.SetActive(false);
			SelectObject.SetActive(true);
		}
		//메인화면 언어창 종료시
		else if (!Settings && !SettingsTwo && Language && !PowerUp && !CharacterSelection)
		{
			Language = false;
			LanguageObject.SetActive(false);
			SelectObject.SetActive(true);
		}
		else if (!Settings && !SettingsTwo && !PowerUp && CharacterSelection && !Teleport)
		{
			CharacterSelection = false;
			CharacterSelectionObject.SetActive(false);
			SelectObject.SetActive(true);
		}
		else if (!Settings && !SettingsTwo && !PowerUp && CharacterSelection && Teleport)
		{
			Teleport = false;
			TeleportObject.SetActive(false);
			CharacterSelectionObject.SetActive(true);
		}
		//업그레이드 설정창 닫기
		else if (!Settings && !SettingsTwo && SettingsThree && !Menu && !PowerUp && !CharacterSelection && !Load && !Inventory)
		{
			SettingsThree = false;
			SelectObject.SetActive(false);
			CurntButton = null;
		}
		//파워업 설정창 닫기
		else if (!Menu && PowerUp && !CharacterSelection)//SettingsThree && 
		{
			CurntButton = SelectButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			SelectObject.SetActive(true);
			PowerUpObject.SetActive(false);
			PowerUp = false;
		}
		//설정창에서 인벤토리 닫기
		else if (SettingsThree && !Menu && Inventory && !CharacterSelection)//
		{
			CurntButton = SelectButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			SelectObject.SetActive(true);
			InventoryObject.SetActive(false);
			Inventory = false;
		}
		// 인게임 '설정' 메뉴에 있을 때, '일시정지' 메뉴로 돌아갑니다.
		else if (Settings && !SettingsTwo && !Menu && !PowerUp && !CharacterSelection)
		{
			CurntButton = PauseButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			PauseObject.SetActive(true);
			SettingsObject.SetActive(false);
			Settings = false;
		}
		// '일시정지' 메뉴에 있을 때, 게임으로 돌아갑니다.
		else if (!Settings && !SettingsTwo && Pause && !Menu && !PowerUp && !CharacterSelection && !Save)
		{
			EventSystem.current.SetSelectedGameObject(null);
			Pause = false;
			PauseObject.SetActive(false);
			Settings = false;
			CurntButton = null;
			//	AbilitiesManager.Instance.StopMoveing = false;
			//Time.timeScale = 1;

		}

		// 게임 플레이 중 '뒤로가기'를 누르면 '일시정지' 메뉴를 엽니다.
		else if (!Settings && !SettingsTwo && !Pause && !Menu && !PowerUp && !CharacterSelection && !Save)
		{
			CurntButton = PauseButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			Pause = true;
			PauseObject.SetActive(true);


		}/****************************************/
		// 메인 메뉴의 '오디오' 또는 '그래픽' 설정 화면에 있을 때, '설정' 메뉴로 돌아갑니다.
		else if (Settings && SettingsTwo && Menu && !PowerUp && !CharacterSelection)
		{

			CurntButton = SettingsButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			AudioObject.SetActive(false);
			GraphicsObject.SetActive(false);
			SettingsObject.SetActive(true);
			SettingsTwo = false;
		}
		else if (SettingsTwo && !Menu && !PowerUp && !CharacterSelection)
		{
			AudioObject.SetActive(false);
			GraphicsObject.SetActive(false);
			SelectObject.SetActive(true);
			SettingsTwo = false;
		}
		// 메인 메뉴의 '설정' 화면에 있을 때, 메인 메뉴로 돌아갑니다.
		else if (Settings && !SettingsTwo && Menu && !PowerUp && !CharacterSelection)
		{
			CurntButton = MaineMenuButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			MaineMenuObject.SetActive(true);
			SettingsObject.SetActive(false);
			Settings = false;

		}
		// '캐릭터 선택' 화면에 있을 때, 메인 메뉴로 돌아갑니다.
		// else if (Menu && CharacterSelection)
		// {
		// 	CharacterSelectionObject.SetActive(false);
		// 	CurntButton = MaineMenuButton;
		// 	EventSystem.current.SetSelectedGameObject(CurntButton);
		// 	MaineMenuObject.SetActive(true);
		// 	CharacterSelection = false;
		// }
		// '파워업' 화면에 있을 때, 메인 메뉴로 돌아갑니다.
		else if (Menu && PowerUp)
		{
			PowerUpObject.SetActive(false);
			CurntButton = MaineMenuButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			MaineMenuObject.SetActive(true);
			CharacterSelection = false;
			PowerUp = false;
		}
		// 인게임 '불러오기' 화면에 있을 때, '일시정지' 메뉴로 돌아갑니다.
		else if (Save && Pause && !Menu && !PowerUp && !CharacterSelection)
		{
			CurntButton = PauseButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			PauseObject.SetActive(true);
			LoadObject.SetActive(false);
			Save = false;
		}
	}
	public void InventoryButton()
	{
		if (!InventoryObject.activeSelf)
		{
			Inventory = true;
			InventoryObject.SetActive(true);
			if(!ingame && SelectObject.activeSelf)
				SelectObject.SetActive(false);
		}
	}
	public void InventoryButtonInGame()
	{
		Inventory = true;
		InventoryObject.SetActive(true);
	}


	public void NewGame()
	{
		CurntButton = CharacterSelectionButton;
		EventSystem.current.SetSelectedGameObject(CurntButton);
		CharacterSelection = true;
		MaineMenuObject.SetActive(false);
		CharacterSelectionObject.SetActive(true);
		//SceneManager.LoadScene(1);

	}
	public void Practice()
	{

		SceneManager.LoadScene(3);

	}

	public void LoadScene(int scene = 0)
	{

		SceneManager.LoadScene(scene);

	}
	public void EndDungeun()
	{
		InventoryManager.Instance.StoreInventoryFrom("ClearInventory");
		SceneManager.LoadScene("Village");
	}
}
