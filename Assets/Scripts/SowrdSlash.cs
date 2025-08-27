using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SowrdSlash : MonoBehaviour
{
   public int SlashCount;
   [SerializeField] private Transform[] slashPostion;
   [SerializeField] private float coolDown = 2;
   [SerializeField] private GameObject slashPrefab ;
    public float damage = 5;
    private float coolDownTimer;
    private void FixedUpdate()
    {
        if(coolDownTimer <= 0)
        {
            coolDownTimer = (coolDown-(PlayerStats.Instance.cooldownReduction* coolDown));
            for (int i = 0; i < SlashCount; i++) 
            {
                GameObject ob =  Instantiate(slashPrefab,slashPostion[i].position,Quaternion.identity, slashPostion[i]);      
                ob.GetComponent<DoDamage>().damage = damage;
            }
        }
        coolDownTimer -= Time.deltaTime;
    }
}
