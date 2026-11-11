using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ClearButton : MonoBehaviour
{
	public void EndDungeun()
	{
		if (LoadingManager.Instance == null)
		{
			Debug.LogError("[ClearButton] LoadingManager is missing.");
			return;
		}
		if (InventoryManager.Instance == null || !InventoryManager.Instance.StoreInventoryFrom("ClearInventory"))
		{
			Debug.LogError("[ClearButton] Items could not be stored. Return to village was cancelled.");
			return;
		}
		if(GameManager.Instance != null)
			GameManager.Instance.StopMoveing = false;
		if(LoadingManager.Instance != null)
			LoadingManager.Instance.LoadScene("Village");
	}
}
