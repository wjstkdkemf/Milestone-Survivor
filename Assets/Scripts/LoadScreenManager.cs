using UnityEngine;
using UnityEngine.UI;

public class LoadScreenManager : MonoBehaviour
{
    public static LoadScreenManager Instance { get; private set; }

    public LoadSceneSelectionButton currentlySelectedSlot;
    public Button confirmButton;

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
    }

    private void Start()
    {
        // Disable the confirm button initially
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
    }

    public void SelectSlot(LoadSceneSelectionButton slot)
    {
        Debug.Log(slot);
        // Deselect the previously selected slot, if any
        if (currentlySelectedSlot != null)
        {
            currentlySelectedSlot.Deselect();
        }

        // Select the new slot
        currentlySelectedSlot = slot;
        currentlySelectedSlot.Select();

        // Enable the confirm button
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }
    }

    public void ConfirmSelectionLoad()
    {
        if (currentlySelectedSlot != null)
        {
            Debug.Log("Confirm button pressed. Loading data for slot: " + currentlySelectedSlot.slotId);

            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.LoadGame(currentlySelectedSlot.slotId);
            }
            else
            {
                Debug.LogError("SaveLoadManager instance not found!");
            }

            // --- YOU WILL IMPLEMENT YOUR LOADING LOGIC HERE ---
            // Example: SaveLoadManager.Instance.LoadGame(currentlySelectedSlot.slotId);
            // After loading data, you would typically load the corresponding scene.
            // UnityEngine.SceneManagement.SceneManager.LoadScene("YourGameScene");
        }
        else
        {
            Debug.LogWarning("No load slot selected.");
        }
    }
    public void ConfirmSelectionSave()
    {
        if (currentlySelectedSlot != null)
        {
            Debug.Log("Confirm button pressed. Saving data for slot: " + currentlySelectedSlot.slotId);

            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.SaveGame(currentlySelectedSlot.slotId);
            }
            else
            {
                Debug.LogError("SaveLoadManager instance not found!");
            }

            // --- YOU WILL IMPLEMENT YOUR LOADING LOGIC HERE ---
            // Example: SaveLoadManager.Instance.LoadGame(currentlySelectedSlot.slotId);
            // After loading data, you would typically load the corresponding scene.
            // UnityEngine.SceneManagement.SceneManager.LoadScene("YourGameScene");
        }
        else
        {
            Debug.LogWarning("No load slot selected.");
        }
    }
}
