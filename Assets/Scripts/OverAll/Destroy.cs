using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    public float Time = .5f;
    public bool DontDestroy;
    // Start is called before the first frame update
    void Start()
    {
        if(!DontDestroy)
        Destroy(gameObject, Time);
    }
    public void DestroyObject()
    {

        Destroy(gameObject);
    }
}
