using UnityEngine;

public class MobileControlsVisibility : MonoBehaviour
{
    [SerializeField] private bool showInEditor = true;
    [SerializeField] private bool forceShow;
    [SerializeField] private bool forceHide;

    private void Awake()
    {
        if (forceHide)
        {
            gameObject.SetActive(false);
            return;
        }

        if (forceShow)
        {
            gameObject.SetActive(true);
            return;
        }

#if UNITY_ANDROID || UNITY_IOS
        gameObject.SetActive(true);
#else
        gameObject.SetActive(showInEditor);
#endif
    }
}