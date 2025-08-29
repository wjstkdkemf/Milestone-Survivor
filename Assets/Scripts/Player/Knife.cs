using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : MonoBehaviour
{
    public float speed = 10f;

    private Vector2 direction;

    // Initialize Knife with a Direction
    public void Initialize(Vector2 direction,float angle)
    {
        this.direction = direction.normalized;
        transform.GetChild(0).transform.rotation= Quaternion.Euler(0,0,angle);
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        transform.Translate(direction * (speed*(1+PlayerStats.Instance.projectileSpeedBonus)) * Time.deltaTime);
    }


}