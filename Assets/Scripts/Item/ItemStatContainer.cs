using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using InventorySystem;
using UnityEngine.Localization.SmartFormat.Utilities;


public class ItemStatContainer : MonoBehaviour
{
    public TextMeshProUGUI enchantStat;
    public TextMeshProUGUI enchantBefore;
    public TextMeshProUGUI enchantAfter;
    public GameObject Cursor;
    [SerializeField]private bool InGame = false;

    public void SetStatImage(StatModifier stat, int Level , bool IsMax = false)
    {
        gameObject.SetActive(true);
        int RealLevel = Level + 1;
        if(IsMax || InGame)
        {
            enchantStat.text = stat.statName;
            enchantBefore.text = (stat.value * RealLevel).ToString();
        }
        else
        {
            enchantStat.text = stat.statName;
            enchantBefore.text = (stat.value * RealLevel).ToString();
            enchantAfter.text = (stat.value * (RealLevel + 1)).ToString();

            if(Cursor != null)
                Cursor.SetActive(true);
        }
    }
    public void ResetData()
    {
        gameObject.SetActive(false);

        if(Cursor != null)
            Cursor.SetActive(false);

        enchantStat.text = "";
        enchantBefore.text = "";
        enchantAfter.text = "";
    }
}
