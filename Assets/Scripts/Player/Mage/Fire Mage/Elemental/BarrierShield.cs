using UnityEngine;

public class BarrierShield : MonoBehaviour
{
    private GameObject explosionEffect;


    public void Setup(GameObject ExplosionEffect)
    {
        explosionEffect = ExplosionEffect;
    }

    public void PlayExplosion(Vector3 position, float radius)
    {
        if (explosionEffect == null)
            return;

        GameObject effect = Instantiate(explosionEffect, position, Quaternion.identity);

        if (effect.TryGetComponent<BarrierExplosionEffect>(out var visual))
        {
            visual.Play(radius);
        }
        else
        {
            Debug.LogError("BarrierExplosionEffect가 필요합니다.", effect);
            Destroy(effect);
        }
    }
}
