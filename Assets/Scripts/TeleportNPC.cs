
using UnityEngine;

public class TeleportNPC : Interactable
{
    public GameObject teleportUICanvas;

    public override void Interact()
    {
        // For now, we will just activate the teleport UI.
        // We can add a dialogue system here later.
        if (teleportUICanvas != null)
        {
            teleportUICanvas.SetActive(true);
        }
    }
}
