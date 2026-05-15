using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using InventorySystem;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.Localization.SmartFormat.Utilities;


public class ItemStatContainer : MonoBehaviour
{
    public TextMeshProUGUI enchantStat;
    public TextMeshProUGUI enchantBefore;
    public TextMeshProUGUI enchantAfter;
    public GameObject Cursor;
    
    public void SetStatImage(StatModifier stat, int Level , bool IsMax = false)
    {
        if(IsMax)
        {
            enchantStat.text = stat.statName;
            enchantBefore.text = (stat.value * Level).ToString();
        }
        else
        {
            enchantStat.text = stat.statName;
            enchantBefore.text = (stat.value * Level).ToString();
            enchantAfter.text = (stat.value * (Level + 1)).ToString();
            Cursor.SetActive(true);
        }
    }
    public void ResetData()
    {
        this.gameObject.SetActive(false);
        Cursor.SetActive(false);

        enchantStat.text = "";
        enchantBefore.text = "";
        enchantAfter.text = "";
    }
}
