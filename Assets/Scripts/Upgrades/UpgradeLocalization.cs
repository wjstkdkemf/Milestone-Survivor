using UnityEngine.Localization.Settings;

public static class UpgradeLocalization
{
    public const string TableName = "Skill_Table";

    public static string Get(string key, string fallback = null)
    {
        if (string.IsNullOrEmpty(key))
            return fallback ?? string.Empty;

        string localized = LocalizationSettings.StringDatabase.GetLocalizedString(TableName, key);

        if (string.IsNullOrEmpty(localized) || localized == key)
            return fallback ?? key;

        return localized;
    }
}
