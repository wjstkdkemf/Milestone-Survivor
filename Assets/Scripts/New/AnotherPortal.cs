using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AnotherPortal : MonoBehaviour
{
    public string MapName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        SceneManager.LoadScene(MapName);
    }
}
