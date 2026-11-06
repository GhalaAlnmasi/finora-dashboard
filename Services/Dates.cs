using System.Globalization;
using finora_dashboard.Models;

namespace finora_dashboard.Services;

public static class Dates
{
    public static DateDisplayFormat Style { get; set; } = DateDisplayFormat.Medium;

    public static string Format(DateOnly date) => Style switch
    {
        DateDisplayFormat.Long => date.ToString("d MMMM yyyy", CultureInfo.InvariantCulture),
        DateDisplayFormat.Iso => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        _ => date.ToString("d MMM yyyy", CultureInfo.InvariantCulture)
    };

    public static string FormatShort(DateOnly date) => Style switch
    {
        DateDisplayFormat.Long => date.ToString("d MMMM", CultureInfo.InvariantCulture),
        DateDisplayFormat.Iso => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        _ => date.ToString("d MMM", CultureInfo.InvariantCulture)
    };

    public static string FormatMonth(DateOnly month) => Style switch
    {
        DateDisplayFormat.Iso => month.ToString("yyyy-MM", CultureInfo.InvariantCulture),
        _ => month.ToString("MMMM yyyy", CultureInfo.InvariantCulture)
    };

    public static string FormatMonthShort(DateOnly month) => Style switch
    {
        DateDisplayFormat.Iso => month.ToString("yyyy-MM", CultureInfo.InvariantCulture),
        _ => month.ToString("MMM yyyy", CultureInfo.InvariantCulture)
    };

    public static string FormatWeekday(DateTime date) => Style switch
    {
        DateDisplayFormat.Long => date.ToString("dddd, d MMMM yyyy", CultureInfo.InvariantCulture),
        DateDisplayFormat.Iso => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        _ => date.ToString("ddd, d MMM yyyy", CultureInfo.InvariantCulture)
    };
}
