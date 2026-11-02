using finora_dashboard.Services;

namespace finora_dashboard.Models;

public sealed class UserPreferencesDocument
{
    public int Version { get; set; } = 1;
    public string DisplayName { get; set; } = DemoPersona.DisplayName;
    public string City { get; set; } = DemoPersona.City;
    public AppTheme Theme { get; set; } = AppTheme.Light;
    public SarDisplayFormat SarFormat { get; set; } = SarDisplayFormat.CodePrefix;
    public DateDisplayFormat DateFormat { get; set; } = DateDisplayFormat.Medium;
}
