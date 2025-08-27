using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorMovement : MonoBehaviour
{
    public float speed = 5f; // Speed of the meteor

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // Destroy the meteor when it goes off screen
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }
}