using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverImageControl : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public string ProgressID;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (GameProgressManager.Instance.IsUnlocked(ProgressID))
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    public void UpdateRenderer()
    {
        if(spriteRenderer.flipX == true)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }
}
