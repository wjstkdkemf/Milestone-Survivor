using System;

public static class NumberFormatter
{
    private static readonly string[] Suffixes =
    {
        "", "K", "M", "B", "T", "Q", "aa", "ab", "ac"
    };

    public static string Format(long value)
    {
        if (value == long.MinValue)
            return "-" + Format(long.MaxValue);

        if (value < 0)
            return "-" + Format(-value);

        if (value < 1000)
            return value.ToString();

        double shortValue = value;
        int suffixIndex = 0;

        while (shortValue >= 1000 && suffixIndex < Suffixes.Length - 1)
        {
            shortValue /= 1000.0;
            suffixIndex++;
        }

        double rounded = Math.Round(shortValue, 1);

        if (rounded >= 1000 && suffixIndex < Suffixes.Length - 1)
        {
            shortValue /= 1000.0;
            suffixIndex++;
        }

        return shortValue.ToString("0.#") + Suffixes[suffixIndex];
    }
}