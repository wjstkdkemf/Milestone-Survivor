using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyOnLoad_ : MonoBehaviour
{

    public static DontDestroyOnLoad_ Instance;
    public int selectedCharacterIndex;
    private void Awake()
    {
        if (Instance != null)
        {

            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

}
