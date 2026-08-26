using UnityEngine.SceneManagement;
using UnityEngine;

public class NextScene : MonoBehaviour
{
    public int SceneIndex;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (LoadingManager.Instance != null)
                LoadingManager.Instance.LoadScene(SceneIndex);
            else
                SceneManager.LoadScene(SceneIndex);
        }
    }
}
