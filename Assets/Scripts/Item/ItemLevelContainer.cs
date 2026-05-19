using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using InventorySystem;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.Localization.SmartFormat.Utilities;


public class ItemLevelContainer : MonoBehaviour
{
    public TextMeshProUGUI LevelBefore;
    public TextMeshProUGUI LevelAfter;
    public GameObject Cursor;
    
    public void SetLevelImage(int Level , bool IsMax = false)
    {
        gameObject.SetActive(true);
        int RealLevel = Level;
        if(IsMax)
        {
            LevelBefore.text = RealLevel.ToString();
        }
        else
        {
            LevelBefore.text = RealLevel.ToString();
            LevelAfter.text = (RealLevel + 1).ToString();

            if(Cursor != null)
                Cursor.SetActive(true);
        }
    }
    public void ResetData()
    {
        gameObject.SetActive(false);

        if(Cursor != null)
            Cursor.SetActive(false);

        LevelBefore.text = "";
        LevelAfter.text = "";
    }
}
