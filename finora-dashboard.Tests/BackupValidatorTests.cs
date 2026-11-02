using System.Text.Json;
using finora_dashboard.Models;
using finora_dashboard.Services;
using Xunit;

namespace finora_dashboard.Tests;

public class BackupValidatorTests
{
    [Fact]
    public void Validate_RejectsInvalidJson()
    {
        var result = BackupValidator.Validate("{ not json");

        Assert.False(result.IsValid);
        Assert.Contains("not valid JSON", result.Error);
        Assert.Null(result.Backup);
    }

    [Fact]
    public void Validate_RejectsUnsupportedVersion()
    {
        var json = """
            {
              "format": "FINORA",
              "version": 99,
              "finance": { "version": 1, "openingBalance": 0, "transactions": [] },
              "budgets": { "version": 1, "budgets": [] }
            }
            """;

        var result = BackupValidator.Validate(json);

        Assert.False(result.IsValid);
        Assert.Contains("version 99", result.Error);
    }

    [Fact]
    public void Validate_RejectsMissingFormat()
    {
        var result = BackupValidator.Validate("""{"version":1,"finance":{"transactions":[]},"budgets":{"budgets":[]}}""");

        Assert.False(result.IsValid);
        Assert.Contains("not a FINORA backup", result.Error);
    }

    [Fact]
    public void Validate_AcceptsRoundTripDocument()
    {
        var backup = SampleBackup();
        var json = JsonSerializer.Serialize(backup, FinoraJson.PrettyOptions);

        var result = BackupValidator.Validate(json);

        Assert.True(result.IsValid, result.Error);
        Assert.NotNull(result.Backup);
        Assert.Single(result.Backup!.Finance!.Transactions);
        Assert.Single(result.Backup.Budgets!.Budgets);
        Assert.Equal("Ghala Alshammari", result.Backup.Preferences!.DisplayName);
    }

    [Fact]
    public void Validate_RejectsDuplicateBudgetEnvelope()
    {
        var backup = SampleBackup();
        backup.Budgets!.Budgets.Add(new CategoryBudget
        {
            Id = "bdg-dup",
            Category = "Housing",
            Month = new DateOnly(2026, 11, 1),
            Limit = 100m
        });

        var result = BackupValidator.Validate(JsonSerializer.Serialize(backup, FinoraJson.Options));

        Assert.False(result.IsValid);
        Assert.Contains("same category and month", result.Error);
    }

    [Fact]
    public void Validate_DoesNotTreatMarkupAsHtml()
    {
        var backup = SampleBackup();
        backup.Finance!.Transactions[0].Description = "<script>alert(1)</script>";
        var json = JsonSerializer.Serialize(backup, FinoraJson.Options);

        var result = BackupValidator.Validate(json);

        Assert.True(result.IsValid, result.Error);
        Assert.Equal("<script>alert(1)</script>", result.Backup!.Finance!.Transactions[0].Description);
    }

    private static FinoraBackupDocument SampleBackup() => new()
    {
        Format = FinoraBackupDocument.FormatName,
        Version = 1,
        ExportedAt = DateTimeOffset.Parse("2026-11-02T00:00:00Z"),
        AppVersion = PreferencesService.AppVersion,
        Finance = new FinanceStoreDocument
        {
            Version = 1,
            OpeningBalance = 1000m,
            Transactions =
            [
                new Transaction
                {
                    Id = "txn-1",
                    Description = "Rent",
                    Category = "Housing",
                    Type = TransactionType.Expense,
                    Amount = 7200m,
                    Date = new DateOnly(2026, 11, 1)
                }
            ]
        },
        Budgets = new BudgetStoreDocument
        {
            Version = 1,
            Budgets =
            [
                new CategoryBudget
                {
                    Id = "bdg-1",
                    Category = "Housing",
                    Month = new DateOnly(2026, 11, 1),
                    Limit = 7200m
                }
            ]
        },
        Preferences = new UserPreferencesDocument
        {
            DisplayName = "Ghala Alshammari",
            City = "Riyadh",
            Theme = AppTheme.Light,
            SarFormat = SarDisplayFormat.CodePrefix,
            DateFormat = DateDisplayFormat.Medium
        }
    };
}
