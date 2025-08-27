using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UiDisplay : MonoBehaviour
{
    public GameObject Window;
    public float speed=2;
    public bool Open;
    RectTransform rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = Window.GetComponent<RectTransform>();
        StartCoroutine(open());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartCoroutine (open());
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(close());
        }
    }

    IEnumerator open()
    {
        Open = true;
        while (rectTransform.localScale.x < 1&&Open)
        {
            
            Vector3 newScale = rectTransform.localScale;
            newScale.x = Mathf.Min(newScale.x + Time.deltaTime*speed, 1);
            rectTransform.localScale = newScale;
            yield return null;
        }

        // Ensure the scale is exactly 1 after the loop
     /*   Vector3 finalScale = rectTransform.localScale;
        finalScale.x = 1;
        rectTransform.localScale = finalScale;*/
    }
    IEnumerator close()
    {

        Open = false;
        while (rectTransform.localScale.x > 0&&!Open)
        {
            Vector3 newScale = rectTransform.localScale;
            newScale.x = Mathf.Max(newScale.x - Time.deltaTime*speed, 0);
            rectTransform.localScale = newScale;
            yield return null;
        }

        // Ensure the scale is exactly 0 after the loop
      /*  Vector3 finalScale = rectTransform.localScale;
        finalScale.x = 0;
        rectTransform.localScale = finalScale;*/
    }
}
