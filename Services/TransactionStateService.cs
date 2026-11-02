using System.Text.Json;
using System.Text.Json.Serialization;
using finora_dashboard.Models;
using Microsoft.JSInterop;

namespace finora_dashboard.Services;

public sealed class TransactionStateService(IJSRuntime js)
{
    public const string StorageKey = "finora.finance.v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly List<Transaction> _transactions = [];
    private decimal _openingBalance = DemoTransactionSeed.OpeningBalance;
    private bool _loaded;

    public DemoProfile Profile { get; } = new()
    {
        FirstName = "Ghala",
        LastName = "Alshammari",
        City = "Riyadh"
    };

    public bool IsReady => _loaded;
    public event Action? Changed;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    public IReadOnlyList<Transaction> Transactions => _transactions;

    public IReadOnlyList<DateOnly> AvailableMonths
    {
        get
        {
            var months = Completed()
                .Select(t => FirstOfMonth(t.Date))
                .Distinct()
                .ToHashSet();
            months.Add(FirstOfMonth(Today));
            return months.OrderBy(m => m).ToList();
        }
    }

    public DateOnly DefaultMonth
    {
        get
        {
            var current = FirstOfMonth(Today);
            return AvailableMonths.Contains(current) ? current : AvailableMonths[^1];
        }
    }

    public async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        try
        {
            var json = await js.InvokeAsync<string?>("finora.storageGet", StorageKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                Seed();
                await PersistAsync();
            }
            else
            {
                var document = JsonSerializer.Deserialize<FinanceStoreDocument>(json, JsonOptions);
                if (document?.Transactions is null)
                {
                    Seed();
                    await PersistAsync();
                }
                else
                {
                    _openingBalance = document.OpeningBalance;
                    _transactions.Clear();
                    _transactions.AddRange(document.Transactions
                        .Where(t => !string.IsNullOrWhiteSpace(t.Id))
                        .Select(Normalize));
                }
            }
        }
        catch (JSException)
        {
            Seed();
        }
        catch (JsonException)
        {
            Seed();
            await PersistAsync();
        }

        _loaded = true;
        Changed?.Invoke();
    }

    public async Task AddAsync(TransactionDraft draft)
    {
        var transaction = ToTransaction(draft, $"txn-{Guid.NewGuid():N}"[..16]);
        _transactions.Add(transaction);
        await PersistAsync();
        Changed?.Invoke();
    }

    public async Task UpdateAsync(TransactionDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Id))
        {
            return;
        }

        var index = _transactions.FindIndex(t => t.Id == draft.Id);
        if (index < 0)
        {
            return;
        }

        _transactions[index] = ToTransaction(draft, draft.Id);
        await PersistAsync();
        Changed?.Invoke();
    }

    public async Task DeleteAsync(string id)
    {
        _transactions.RemoveAll(t => t.Id == id);
        await PersistAsync();
        Changed?.Invoke();
    }

    public bool IsCompleted(Transaction transaction) => transaction.Date <= Today;

    public MonthlySummary GetSummary(DateOnly month)
    {
        var start = FirstOfMonth(month);
        var end = start.AddMonths(1);
        var previousStart = start.AddMonths(-1);

        var income = SumCompleted(start, end, TransactionType.Income);
        var expenses = SumCompleted(start, end, TransactionType.Expense);
        var previousIncome = SumCompleted(previousStart, start, TransactionType.Income);
        var previousExpenses = SumCompleted(previousStart, start, TransactionType.Expense);
        var savings = income - expenses;
        var previousSavings = previousIncome - previousExpenses;
        var balanceEnd = BalanceAsOf(Min(end.AddDays(-1), Today));
        var balanceStart = BalanceAsOf(start.AddDays(-1));

        return new MonthlySummary
        {
            Month = start,
            TotalBalance = balanceEnd,
            Income = income,
            Expenses = expenses,
            Savings = savings,
            BalanceChangePercent = ChangePercent(balanceStart, balanceEnd),
            IncomeChangePercent = ChangePercent(previousIncome, income),
            ExpenseChangePercent = ChangePercent(previousExpenses, expenses),
            SavingsChangePercent = ChangePercent(previousSavings, savings)
        };
    }

    public IReadOnlyList<TrendPoint> GetTrend(DateOnly month)
    {
        var selected = FirstOfMonth(month);
        var months = AvailableMonths.Where(m => m <= selected).TakeLast(6).ToList();
        if (months.Count == 0)
        {
            months.Add(selected);
        }

        return months.Select(current =>
        {
            var next = current.AddMonths(1);
            return new TrendPoint
            {
                Month = current,
                Income = SumCompleted(current, next, TransactionType.Income),
                Expenses = SumCompleted(current, next, TransactionType.Expense)
            };
        }).ToList();
    }

    public IReadOnlyList<CategoryTotal> GetCategoryBreakdown(DateOnly month)
    {
        var start = FirstOfMonth(month);
        var end = start.AddMonths(1);
        var expenses = Completed()
            .Where(t => t.Type == TransactionType.Expense && t.Date >= start && t.Date < end)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
            .OrderByDescending(g => g.Amount)
            .ToList();

        var total = expenses.Sum(g => g.Amount);
        return expenses.Select(g => new CategoryTotal
        {
            Category = g.Category,
            Amount = g.Amount,
            Color = FinanceCategories.Color(g.Category),
            Share = total == 0 ? 0 : g.Amount / total
        }).ToList();
    }

    public IReadOnlyList<Transaction> GetRecent(DateOnly month, int count = 8)
    {
        var start = FirstOfMonth(month);
        var end = start.AddMonths(1);

        return Completed()
            .Where(t => t.Date >= start && t.Date < end)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Take(count)
            .ToList();
    }

    private IEnumerable<Transaction> Completed() => _transactions.Where(IsCompleted);

    private decimal SumCompleted(DateOnly startInclusive, DateOnly endExclusive, TransactionType type) =>
        Completed()
            .Where(t => t.Type == type && t.Date >= startInclusive && t.Date < endExclusive)
            .Sum(t => t.Amount);

    private decimal BalanceAsOf(DateOnly date) =>
        _openingBalance + Completed().Where(t => t.Date <= date).Sum(t => t.SignedAmount);

    private void Seed()
    {
        _openingBalance = DemoTransactionSeed.OpeningBalance;
        _transactions.Clear();
        _transactions.AddRange(DemoTransactionSeed.Create());
    }

    private async Task PersistAsync()
    {
        var document = new FinanceStoreDocument
        {
            Version = 1,
            OpeningBalance = _openingBalance,
            Transactions = _transactions.Select(t => t.Clone()).ToList()
        };

        var json = JsonSerializer.Serialize(document, JsonOptions);
        await js.InvokeVoidAsync("finora.storageSet", StorageKey, json);
    }

    private static Transaction ToTransaction(TransactionDraft draft, string id)
    {
        if (draft.Amount is null || draft.Date is null || string.IsNullOrWhiteSpace(draft.Description) || string.IsNullOrWhiteSpace(draft.Category))
        {
            throw new InvalidOperationException("Transaction draft is incomplete.");
        }

        return new Transaction
        {
            Id = id,
            Description = draft.Description.Trim(),
            Category = draft.Category,
            Type = draft.Type,
            Amount = Math.Round(draft.Amount.Value, 2, MidpointRounding.AwayFromZero),
            Date = draft.Date.Value
        };
    }

    private static Transaction Normalize(Transaction transaction)
    {
        transaction.Description = string.IsNullOrWhiteSpace(transaction.Description) ? "Untitled" : transaction.Description.Trim();
        transaction.Amount = Math.Abs(transaction.Amount);
        return transaction;
    }

    private static DateOnly FirstOfMonth(DateOnly month) => new(month.Year, month.Month, 1);

    private static DateOnly Min(DateOnly a, DateOnly b) => a <= b ? a : b;

    private static decimal? ChangePercent(decimal previous, decimal current)
    {
        if (previous == 0)
        {
            return current == 0 ? 0 : null;
        }

        return Math.Round((current - previous) / Math.Abs(previous) * 100m, 1);
    }
}
