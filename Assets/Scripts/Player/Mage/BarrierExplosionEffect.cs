using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BarrierExplosionEffect : MonoBehaviour
{
    public void Play(float radius)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer.sprite == null || radius <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        float diameter = radius * 2f;

        transform.localScale = new Vector3(
            diameter / spriteSize.x,
            diameter / spriteSize.y,
            1f
        );

        Destroy(gameObject, 1f);
    }
}