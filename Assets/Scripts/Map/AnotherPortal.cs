using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AnotherPortal : MonoBehaviour
{
    public string MapName;
    private bool isChangingScene = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isChangingScene)
        {
            // 2. 코루틴을 시작
            StartCoroutine(ChangeSceneAfterSaving());
        }
    }
    private IEnumerator ChangeSceneAfterSaving()
    {
        if (string.IsNullOrEmpty(MapName))
        {
            Debug.LogWarning("[AnotherPortal] MapName is empty. Scene change canceled.");
            yield break;
        }

        // 3. 씬 전환을 시작했다고 플래그를 설정 (중복 방지)
        isChangingScene = true;

        Debug.Log("인벤토리 저장을 시작합니다...");

        // 4. 이전에 만든 저장 함수를 호출해 모든 인벤토리를 저장
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SaveAllInventories("Current.json");
        else
            Debug.LogWarning("[AnotherPortal] InventoryManager is missing. Inventory save skipped.");

        // 5. 저장이 완료될 때까지 잠시 대기 (다음 프레임까지)
        // File.WriteAllText는 동기 방식이라 즉시 완료되지만, 안전을 위해 한 프레임 대기합니다.
        yield return null;

        Debug.Log("저장 완료! " + MapName + " 씬으로 전환합니다.");

        // 6. 모든 과정이 끝난 후 씬을 전환
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.LoadScene(MapName);
        else
            SceneManager.LoadScene(MapName);
    }
      
    public void StartButton()
    {
        StartCoroutine(ChangeSceneAfterSaving());
    }
}
