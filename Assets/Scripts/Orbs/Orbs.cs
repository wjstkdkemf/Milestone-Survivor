using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LTGUI;

public class Orbs : MonoBehaviour
{
    public GameObject orbPrefab; // The orb prefab
    public int orbCount = 1; // Number of orbs
    public float radius = 2f; // Radius of the orbit
    public float rotationSpeed = 50f; // Speed of orbiting

    private List<GameObject> orbs = new List<GameObject>(); // List to hold orb instances
    private float angleStep;
    private float chanceDoubleDamage;
    public float BaseDamage = 5;
    public Transform Player;
    void Start()
    {
      
        Invoke("SpawnOrbs", 1);
    }

    void FixedUpdate()
    {
        OrbitOrbs();
    }

    // Method to spawn orbs based on the orbCount
   public void SpawnOrbs()
    {
        ClearOrbs(); // Remove any existing orbs

        angleStep = 360f / orbCount;

        for (int i = 0; i < orbCount; i++)
        {
            float angle = i * angleStep;
            Vector3 orbPosition = GetOrbPosition(angle);
            GameObject orb = Instantiate(orbPrefab, orbPosition, Quaternion.identity, transform);
            orb.GetComponent<IceOrb>().SetInfo(BaseDamage, chanceDoubleDamage);
            orbs.Add(orb);
        }
    }

    // Method to calculate the position of each orb based on the angle
    Vector3 GetOrbPosition(float angle)
    {
        float angleInRadians = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(angleInRadians) * radius;
        float y = Mathf.Sin(angleInRadians) * radius;
        return new Vector3(x, y, 0) + transform.position;
    }

    // Method to orbit the orbs around the player
    void OrbitOrbs()
    {
        for (int i = 0; i < orbs.Count; i++)
        {
            float angle = (i * angleStep) + Time.time * rotationSpeed;
            Vector3 orbPosition = GetOrbPosition(angle);
            orbs[i].transform.position = orbPosition;
        }
    }

    // Clear all existing orbs
    void ClearOrbs()
    {
        foreach (GameObject orb in orbs)
        {
            Destroy(orb);
        }
        orbs.Clear();
    }

    // Method to increase the number of orbs
    public void IncreaseOrbCount(int amount)
    {
        orbCount += amount;
        SpawnOrbs();
    }

    // Method to increase the radius of the orbit
    public void IncreaseRadius(float amount)
    {
        radius += amount;
        SpawnOrbs();
    }

    // Method to increase the speed of orbiting
    public void IncreaseSpeed(float amount)
    {
        rotationSpeed += amount;
    }
}