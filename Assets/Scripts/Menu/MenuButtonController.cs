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
    public GameObject LoadObject;
	public bool Menu, Pause, Settings, SettingsTwo, CharacterSelection,PowerUp, Load;
	public GameObject CurntButton;

	private GameObject MaineMenuButton;
	private GameObject PauseButton;
	private GameObject SettingsButton;
	private GameObject AudioButton;
	private GameObject GraphicsButton;
	public  GameObject PowerUpButton;
    public GameObject CharacterSelectionButton;
    public string FunctionName;

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
		else if (!Pause && !UpgradeObject.activeSelf && UpgradeObject != null && !Menu)
		{
			GameManager.Instance.Pause = false;
			GameManager.Instance.StopMoveing = false;


		}


		if (Input.GetKeyDown(KeyCode.Escape))
		{


			back();


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

	public void Audio()
	{
		CurntButton = AudioButton;
		EventSystem.current.SetSelectedGameObject(CurntButton);
		AudioObject.SetActive(true);
		SettingsObject.SetActive(false);
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

    public void PowerUps()
    {
		CurntButton = PowerUpButton;
        EventSystem.current.SetSelectedGameObject(CurntButton);
        PowerUp = true;
		PowerUpObject.SetActive(true);
        
    }

    public void LoadGame()
    {
        CurntButton = MaineMenuButton;
        EventSystem.current.SetSelectedGameObject(CurntButton);
        Load = true;
        MaineMenuObject.SetActive(false);
        LoadObject.SetActive(true);
    }

    public void back()
	{
		if (Settings && SettingsTwo && !Menu && !PowerUp && !CharacterSelection)
		{
			CurntButton = SettingsButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			AudioObject.SetActive(false);
			GraphicsObject.SetActive(false);
			SettingsObject.SetActive(true);
			SettingsTwo = false;

		}
		else if (Settings && !SettingsTwo && !Menu && !PowerUp && !CharacterSelection)
		{
			CurntButton = PauseButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			PauseObject.SetActive(true);
			SettingsObject.SetActive(false);
			Settings = false;

		}
		else if (!Settings && !SettingsTwo && Pause && !Menu && !PowerUp && !CharacterSelection && !Load)
		{
			//EventSystem.current.SetSelectedGameObject(null);
			Pause = false;
			PauseObject.SetActive(false);
			Settings = false;
			CurntButton = null;
			//	AbilitiesManager.Instance.StopMoveing = false;
			//Time.timeScale = 1;

		}

		else if (!Settings && !SettingsTwo && !Pause && !Menu && !PowerUp && !CharacterSelection)
		{
			CurntButton = PauseButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			Pause = true;
			PauseObject.SetActive(true);


		}/****************************************/
		else if (Settings && SettingsTwo && Menu && !PowerUp && !CharacterSelection)
		{

			CurntButton = SettingsButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			AudioObject.SetActive(false);
			GraphicsObject.SetActive(false);
			SettingsObject.SetActive(true);
			SettingsTwo = false;

		}
		else if (Settings && !SettingsTwo && Menu && !PowerUp && !CharacterSelection)
		{
			CurntButton = MaineMenuButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			MaineMenuObject.SetActive(true);
			SettingsObject.SetActive(false);
			Settings = false;

		}
		else if (Menu && CharacterSelection)
		{
			CharacterSelectionObject.SetActive(false);
			CurntButton = MaineMenuButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			MaineMenuObject.SetActive(true);
			CharacterSelection = false;
		}
		else if (Menu && PowerUp)
		{
			PowerUpObject.SetActive(false);
			//  CurntButton = MaineMenuButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			MaineMenuObject.SetActive(true);
			CharacterSelection = false;
		}
		else if (Load && !Menu && !PowerUp && !CharacterSelection)
		{
			CurntButton = PauseButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			PauseObject.SetActive(true);
			LoadObject.SetActive(false);
			Load = false;
		}
		else if (Menu && Load)
		{
			LoadObject.SetActive(false);
			CurntButton = MaineMenuButton;
			EventSystem.current.SetSelectedGameObject(CurntButton);
			MaineMenuObject.SetActive(true);
			Load = false;
		}


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
}
