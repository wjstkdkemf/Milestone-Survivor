using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebootSetting : MonoBehaviour
{
    public static RebootSetting Instance;

    public void Start()
    {
        // If an instance already exists and it's not this one, destroy this new one.
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return; // Stop further execution
        }

        // This is the first instance.
        Instance = this;
        DontDestroyOnLoad(this.gameObject); // Persist this object across scenes

        // --- Perform your one-time setup here ---
        Debug.Log("RebootSetting: 첫 인스턴스 생성 및 1회 설정 실행");
        // You might need to ensure GameManager.Instance is ready before this is called.
        GameManager.Instance.GetComponent<CharacterSelection>().OnceSetting();
    }
}
