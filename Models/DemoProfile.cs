namespace finora_dashboard.Models;

public sealed class DemoProfile
{
    public string DisplayName { get; set; } = "Ghala Alshammari";
    public string City { get; set; } = "Riyadh";

    public string FirstName
    {
        get
        {
            var parts = DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length == 0 ? "There" : parts[0];
        }
    }

    public string FullName => string.IsNullOrWhiteSpace(DisplayName) ? "FINORA" : DisplayName.Trim();

    public string Initials => InitialsFrom(DisplayName);

    public static string InitialsFrom(string? displayName)
    {
        var parts = (displayName ?? "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return "FN";
        }

        if (parts.Length == 1)
        {
            var word = parts[0];
            return word.Length >= 2
                ? $"{char.ToUpperInvariant(word[0])}{char.ToUpperInvariant(word[1])}"
                : char.ToUpperInvariant(word[0]).ToString();
        }

        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }
}
