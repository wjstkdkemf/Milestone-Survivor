using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyDelay : MonoBehaviour
{
    public float Time = .5f;
    public bool DontDestroy;

    private void OnEnable()
    {
        if (!DontDestroy)
        {
            StartCoroutine(ReturnAfterTime());
        }
    }

    private IEnumerator ReturnAfterTime()
    {
        yield return new WaitForSeconds(Time);
        DestroyObject();
    }

    public void DestroyObject()
    {
        if (ObjectPoolingManager.Instance != null)
        {
            ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
        }
        else
        {
            Destroy(gameObject); 
        }
    }
}
