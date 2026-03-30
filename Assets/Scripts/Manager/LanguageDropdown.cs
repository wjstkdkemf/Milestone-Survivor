using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.Localization.Settings; 
using UnityEngine.Localization;

public class LanguageDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    void Start()
    {
        StartCoroutine(SetupDropdown());
    }

    IEnumerator SetupDropdown()
    {
        yield return LocalizationSettings.InitializationOperation;

        dropdown.ClearOptions();

        var locales = LocalizationSettings.AvailableLocales.Locales;
        List<string> options = new List<string>();
        int currentLocaleIndex = 0;

        for (int i = 0; i < locales.Count; i++)
        {
            Locale locale = locales[i];

            string displayName = locale.Identifier.CultureInfo.NativeName;

            options.Add(displayName);

            if (locale == LocalizationSettings.SelectedLocale)
            {
                currentLocaleIndex = i;
            }
        }

        dropdown.AddOptions(options);
        dropdown.value = currentLocaleIndex;
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}