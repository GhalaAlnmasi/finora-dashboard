using System.Globalization;

namespace finora_dashboard.Services;

public static class Money
{
    public static string Sar(decimal amount) =>
        string.Create(CultureInfo.InvariantCulture, $"SAR {amount:N2}");

    public static string CompactSar(decimal amount)
    {
        var abs = Math.Abs(amount);
        if (abs >= 1_000_000m)
        {
            return string.Create(CultureInfo.InvariantCulture, $"SAR {amount / 1_000_000m:0.#}M");
        }

        if (abs >= 10_000m)
        {
            return string.Create(CultureInfo.InvariantCulture, $"SAR {amount / 1_000m:0.#}k");
        }

        return Sar(amount);
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
