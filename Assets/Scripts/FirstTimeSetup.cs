using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstTimeSetup : MonoBehaviour
{
    private const string FirstTimeKey = "HasOpenedBefore";
    [SerializeField] private GameObject GameManager;
    void Start()
    {
        if (!PlayerPrefs.HasKey(FirstTimeKey))
        {
            // First time running the game
            RunFirstTimeSetup();

            // Set the flag to indicate the game has been opened
            PlayerPrefs.SetInt(FirstTimeKey, 1);
            PlayerPrefs.Save(); // Save changes to PlayerPrefs
        }
        else
        {
            Debug.Log("Game has been opened before.");
        }
    }

    private void RunFirstTimeSetup()
    {
        GameManager.GetComponent<CharacterSelectionManager>().ResetCharacters();
        GameManager.GetComponent<PowerUpManager>().RefundPowerUp();
        GameManager.GetComponent<StageSelection>().ResetStages();
    }
}
