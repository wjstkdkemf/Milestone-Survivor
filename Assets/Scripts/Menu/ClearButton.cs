using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearButton : MonoBehaviour
{
	public void EndDungeun()
	{
		GameManager.Instance.Pause = false;
		GameManager.Instance.StopMoveing = false;

		InventoryManager.Instance.StoreInventoryFrom("ClearInventory");
		WaveSpawner.Instance.ReleaseWaveAssets();
		ObjectPoolingManager.Instance.ClearAllPools();
		LoadingManager.Instance.LoadScene("Village");
	}
}
