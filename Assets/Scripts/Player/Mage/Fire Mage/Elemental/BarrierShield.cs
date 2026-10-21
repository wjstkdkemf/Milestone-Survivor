using UnityEngine;

public class BarrierShield : MonoBehaviour
{
    private BarrierWeapon parentWeapon;
    private GameObject explosionEffect;


    public void Setup(BarrierWeapon weapon, GameObject ExplosionEffect)
    {
        parentWeapon = weapon;
        explosionEffect = ExplosionEffect;
    }

    public void PlayExplosion()
    {
        if(explosionEffect == null)
            return;
        
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
    }
}
