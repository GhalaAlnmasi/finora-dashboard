using System.Text.Json;
using finora_dashboard.Models;

namespace finora_dashboard.Services;

public static class BackupValidator
{
    public static BackupValidationResult Validate(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return BackupValidationResult.Fail("The file is empty. Choose a FINORA backup JSON file.");
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return BackupValidationResult.Fail("That file is not valid JSON. FINORA only imports backups it exported.");
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return BackupValidationResult.Fail("The backup must be a JSON object, not a list or value.");
            }

            if (!TryGetString(document.RootElement, "format", out var format) ||
                !string.Equals(format, FinoraBackupDocument.FormatName, StringComparison.OrdinalIgnoreCase))
            {
                return BackupValidationResult.Fail("This is not a FINORA backup. The format marker is missing or unsupported.");
            }

            if (!TryGetInt(document.RootElement, "version", out var version))
            {
                return BackupValidationResult.Fail("The backup version is missing.");
            }

            if (version != FinoraBackupDocument.CurrentVersion)
            {
                return BackupValidationResult.Fail($"This backup uses version {version}, which this demo cannot import. Supported version: {FinoraBackupDocument.CurrentVersion}.");
            }
        }

        FinoraBackupDocument backup;
        try
        {
            backup = JsonSerializer.Deserialize<FinoraBackupDocument>(json, FinoraJson.Options)
                     ?? throw new JsonException("empty");
        }
        catch (JsonException)
        {
            return BackupValidationResult.Fail("The backup JSON could not be read. No data was changed.");
        }

        if (backup.Finance is null || backup.Budgets is null)
        {
            return BackupValidationResult.Fail("The backup is missing transactions or budgets. Import was cancelled.");
        }

        var financeError = ValidateFinance(backup.Finance);
        if (financeError is not null)
        {
            return BackupValidationResult.Fail(financeError);
        }

        var budgetError = ValidateBudgets(backup.Budgets);
        if (budgetError is not null)
        {
            return BackupValidationResult.Fail(budgetError);
        }

        if (backup.Preferences is not null)
        {
            var prefsError = ValidatePreferences(backup.Preferences);
            if (prefsError is not null)
            {
                return BackupValidationResult.Fail(prefsError);
            }
        }

        return BackupValidationResult.Ok(backup);
    }

    private static string? ValidateFinance(FinanceStoreDocument finance)
    {
        if (finance.Transactions is null)
        {
            return "Transaction data is missing from the backup.";
        }

        if (finance.Transactions.Count > 5_000)
        {
            return "The backup contains more transactions than this demo allows.";
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in finance.Transactions)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                return "A transaction is missing an id.";
            }

            if (!ids.Add(item.Id))
            {
                return "The backup contains duplicate transaction ids.";
            }

            if (string.IsNullOrWhiteSpace(item.Description) || item.Description.Length > 200)
            {
                return "A transaction description is missing or too long.";
            }

            if (string.IsNullOrWhiteSpace(item.Category) || item.Category.Length > 80)
            {
                return "A transaction category is missing or too long.";
            }

            if (item.Amount <= 0 || item.Amount > 100_000_000m)
            {
                return "A transaction amount is outside the allowed range.";
            }

            if (item.Type is not TransactionType.Income and not TransactionType.Expense)
            {
                return "A transaction has an unknown type.";
            }

            if (item.Date == default)
            {
                return "A transaction is missing a date.";
            }
        }

        return null;
    }

    private static string? ValidateBudgets(BudgetStoreDocument budgets)
    {
        if (budgets.Budgets is null)
        {
            return "Budget data is missing from the backup.";
        }

        if (budgets.Budgets.Count > 1_000)
        {
            return "The backup contains more budgets than this demo allows.";
        }

        var keys = new HashSet<(string Category, DateOnly Month)>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in budgets.Budgets)
        {
            if (string.IsNullOrWhiteSpace(item.Id) || !ids.Add(item.Id))
            {
                return "A budget is missing an id or uses a duplicate id.";
            }

            if (string.IsNullOrWhiteSpace(item.Category) || item.Category.Length > 80)
            {
                return "A budget category is missing or too long.";
            }

            if (item.Limit <= 0 || item.Limit > 100_000_000m)
            {
                return "A budget limit is outside the allowed range.";
            }

            if (item.Month == default)
            {
                return "A budget is missing a month.";
            }

            var month = BudgetMath.FirstOfMonth(item.Month);
            if (!keys.Add((item.Category.Trim(), month)))
            {
                return "The backup contains two envelopes for the same category and month.";
            }
        }

        return null;
    }

    private static string? ValidatePreferences(UserPreferencesDocument prefs)
    {
        if (string.IsNullOrWhiteSpace(prefs.DisplayName) || prefs.DisplayName.Trim().Length > 80)
        {
            return "The saved display name is missing or too long.";
        }

        if (string.IsNullOrWhiteSpace(prefs.City) || prefs.City.Trim().Length > 80)
        {
            return "The saved city is missing or too long.";
        }

        if (!Enum.IsDefined(prefs.Theme) ||
            !Enum.IsDefined(prefs.SarFormat) ||
            !Enum.IsDefined(prefs.DateFormat))
        {
            return "A preference value in the backup is not supported.";
        }

        return null;
    }

    private static bool TryGetString(JsonElement root, string name, out string value)
    {
        value = "";
        if (!root.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? "";
        return true;
    }

    private static bool TryGetInt(JsonElement root, string name, out int value)
    {
        value = 0;
        if (!root.TryGetProperty(name, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
        {
            return true;
        }

        return property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out value);
    }
}
