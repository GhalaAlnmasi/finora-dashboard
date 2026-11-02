namespace finora_dashboard.Models;

public sealed class FinoraBackupDocument
{
    public const string FormatName = "FINORA";
    public const int CurrentVersion = 1;

    public string Format { get; set; } = FormatName;
    public int Version { get; set; } = CurrentVersion;
    public DateTimeOffset ExportedAt { get; set; }
    public string AppVersion { get; set; } = "";
    public FinanceStoreDocument? Finance { get; set; }
    public BudgetStoreDocument? Budgets { get; set; }
    public UserPreferencesDocument? Preferences { get; set; }
}
