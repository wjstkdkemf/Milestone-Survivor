using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Texture2D cursorTexture2;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;

    void Start()
    {
        hotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
    }

    private void Update()
    {
        if (GameManager.Instance.Pause)
        {
            Cursor.SetCursor(cursorTexture2, hotspot, cursorMode);
        }
        else
        {
            Cursor.SetCursor(cursorTexture, hotspot, cursorMode);

        }
    }

}