using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ClearButton : MonoBehaviour
{
	public void EndDungeun()
	{
		if(GameManager.Instance != null)
		{
		GameManager.Instance.Pause = false;
		GameManager.Instance.StopMoveing = false;
		}
		if(InventoryManager.Instance != null)
			InventoryManager.Instance.StoreInventoryFrom("ClearInventory");
		if(WaveSpawner.Instance != null)
			WaveSpawner.Instance.ReleaseWaveAssets();
		if(ObjectPoolingManager.Instance != null)
			ObjectPoolingManager.Instance.ClearAllPools();
		if(LoadingManager.Instance != null)
			LoadingManager.Instance.LoadScene("Village");
		else
			SceneManager.LoadScene("Village");
	}
}
