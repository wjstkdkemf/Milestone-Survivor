using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalChanger : Interactable
{
    public string ProgressID;
    public ConditionalObject changingObject;
    public override void Interact()
    {
        if (GameProgressManager.Instance.IsUnlocked(ProgressID))
        {
            GameProgressManager.Instance.Dislock(ProgressID);
        }
        else
        {
            GameProgressManager.Instance.Unlock(ProgressID);
        }

        changingObject.CheckProgess();
    }
}
