using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GassSpawner : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private float spawnRate=.1f;

    void Start()
    {
        StartCoroutine(SpawnObjectEveryTwoSeconds(spawnRate));
    }

    IEnumerator SpawnObjectEveryTwoSeconds(float spawnRate)
    {
        while (true)
        {
          GameObject newObject =   Instantiate(objectToSpawn, transform.position, Quaternion.identity);
           // newObject.transform.parent = transform;
            yield return new WaitForSeconds(spawnRate);
        }
    }
}