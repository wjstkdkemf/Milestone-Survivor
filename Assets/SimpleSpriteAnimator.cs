using UnityEngine;

public class SimpleSpriteAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer target;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 8f;

    private int index;
    private float timer;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || target == null)
            return;

        timer += Time.deltaTime;

        float frameTime = 1f / fps;
        if (timer >= frameTime)
        {
            timer -= frameTime;
            index = (index + 1) % frames.Length;
            target.sprite = frames[index];
        }
    }
}