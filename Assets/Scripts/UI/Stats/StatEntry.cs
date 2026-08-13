using UnityEngine;

public enum StatValueFormat
{
    Number,
    Percent,
    CurrentMax
}

public enum StatEntryKind
{
    Normal,
    Positive,
    Negative,
    Header,
    Empty
}

public class StatEntry
{
    public string StatKey { get; private set; }
    public string Label { get; private set; }
    public string ValueText { get; private set; }
    public Sprite Icon { get; private set; }
    public StatEntryKind Kind { get; private set; }

    public bool HasValue => !string.IsNullOrEmpty(ValueText);

    public StatEntry(string statKey, string label, string valueText, StatEntryKind kind = StatEntryKind.Normal, Sprite icon = null)
    {
        StatKey = statKey;
        Label = label;
        ValueText = valueText;
        Kind = kind;
        Icon = icon;
    }

    public static StatEntry Header(string localizationKey, string fallback)
    {
        return new StatEntry(localizationKey, fallback, "", StatEntryKind.Header);
    }

    public static StatEntry Empty(string localizationKey, string fallback)
    {
        return new StatEntry(localizationKey, fallback, "", StatEntryKind.Empty);
    }

    public static StatEntry Number(string statKey, float value, Sprite icon = null)
    {
        return new StatEntry(statKey, CharacterLocalization.GetStatLabel(statKey), FormatNumber(value), StatEntryKind.Normal, icon);
    }

    public static StatEntry Percent(string statKey, float value, Sprite icon = null)
    {
        return new StatEntry(statKey, CharacterLocalization.GetStatLabel(statKey), $"{FormatNumber(value)}%", StatEntryKind.Normal, icon);
    }

    public static StatEntry Signed(string statKey, float value, Sprite icon = null)
    {
        StatEntryKind kind = value > 0f ? StatEntryKind.Positive : value < 0f ? StatEntryKind.Negative : StatEntryKind.Normal;
        return new StatEntry(statKey, CharacterLocalization.GetStatModifierLabel(statKey), FormatSigned(value), kind, icon);
    }

    public static StatEntry CurrentMax(string statKey, float current, float max, Sprite icon = null)
    {
        string value = $"{FormatNumber(current)} / {FormatNumber(max)}";
        return new StatEntry(statKey, CharacterLocalization.GetStatLabel(statKey), value, StatEntryKind.Normal, icon);
    }

    public static string FormatNumber(float value)
    {
        return value.ToString("0.##");
    }

    public static string FormatSigned(float value)
    {
        return value.ToString("+0.##;-0.##;0");
    }
}
