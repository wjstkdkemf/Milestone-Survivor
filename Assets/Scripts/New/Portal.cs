using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public GameObject Next_Portal;

    public Vector3 Next_pos = new Vector3(0, 0, 0);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Next_Portal != null)
        {
            Transform trans = other.gameObject.GetComponent<Transform>();
            Debug.Log(Next_Portal.GetComponent<Portal>().Next_pos);
            trans.position = Next_Portal.GetComponent<Portal>().Next_pos;
        }
    }
}
