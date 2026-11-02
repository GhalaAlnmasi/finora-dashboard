namespace finora_dashboard.Models;

public sealed class UserPreferencesDocument
{
    public int Version { get; set; } = 1;
    public string DisplayName { get; set; } = "Ghala Alshammari";
    public string City { get; set; } = "Riyadh";
    public AppTheme Theme { get; set; } = AppTheme.Light;
    public SarDisplayFormat SarFormat { get; set; } = SarDisplayFormat.CodePrefix;
    public DateDisplayFormat DateFormat { get; set; } = DateDisplayFormat.Medium;
}
