using UnityEngine;
using Cinemachine;
public class EncounterCamera : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;

    void Awake() 
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && vcam != null)
        {
            vcam.Follow = player.transform;
            //vcam.LookAt = player.transform; 
        }
    }
}
