
using UnityEngine;

public class LightningSpark : MonoBehaviour
{
    public float lifetime = 1.0f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
