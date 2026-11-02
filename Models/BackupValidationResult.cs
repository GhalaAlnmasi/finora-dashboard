namespace finora_dashboard.Models;

public sealed class BackupValidationResult
{
    public bool IsValid { get; init; }
    public string Error { get; init; } = "";
    public FinoraBackupDocument? Backup { get; init; }

    public static BackupValidationResult Ok(FinoraBackupDocument backup) =>
        new() { IsValid = true, Backup = backup };

    public static BackupValidationResult Fail(string error) =>
        new() { IsValid = false, Error = error };
}
