using System.Globalization;
using finora_dashboard.Models;

namespace finora_dashboard.Services;

public static class Money
{
    public static SarDisplayFormat Style { get; set; } = SarDisplayFormat.CodePrefix;

    public static string Sar(decimal amount)
    {
        var number = Style == SarDisplayFormat.UngroupedPrefix
            ? amount.ToString("0.00", CultureInfo.InvariantCulture)
            : amount.ToString("N2", CultureInfo.InvariantCulture);

        return Style == SarDisplayFormat.CodeSuffix
            ? $"{number} SAR"
            : $"SAR {number}";
    }

    public static string CompactSar(decimal amount)
    {
        var abs = Math.Abs(amount);
        string number;
        if (abs >= 1_000_000m)
        {
            number = (amount / 1_000_000m).ToString("0.#", CultureInfo.InvariantCulture) + "M";
        }
        else if (abs >= 10_000m)
        {
            number = (amount / 1_000m).ToString("0.#", CultureInfo.InvariantCulture) + "k";
        }
        else
        {
            return Sar(amount);
        }

        return Style == SarDisplayFormat.CodeSuffix
            ? $"{number} SAR"
            : $"SAR {number}";
    }

    public static string Percent(decimal? value)
    {
        if (value is null)
        {
            return "—";
        }

        var sign = value.Value > 0 ? "+" : string.Empty;
        return string.Create(CultureInfo.InvariantCulture, $"{sign}{value.Value:0.0}%");
    }
}
