using System.Text.Json;
using System.Text.Json.Serialization;
using finora_dashboard.Models;
using Microsoft.JSInterop;

namespace finora_dashboard.Services;

public sealed class BudgetStateService
{
    public const string StorageKey = "finora.budgets.v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IJSRuntime _js;
    private readonly TransactionStateService _transactions;
    private readonly List<CategoryBudget> _budgets = [];
    private bool _loaded;

    public BudgetStateService(IJSRuntime js, TransactionStateService transactions)
    {
        _js = js;
        _transactions = transactions;
        _transactions.Changed += () => Changed?.Invoke();
    }

    public bool IsReady => _loaded;
    public event Action? Changed;
    public IReadOnlyList<CategoryBudget> Budgets => _budgets;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        await _transactions.EnsureLoadedAsync();

        try
        {
            var json = await _js.InvokeAsync<string?>("finora.storageGet", StorageKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                Seed();
                await PersistAsync();
            }
            else
            {
                var document = JsonSerializer.Deserialize<BudgetStoreDocument>(json, JsonOptions);
                if (document?.Budgets is null)
                {
                    Seed();
                    await PersistAsync();
                }
                else
                {
                    _budgets.Clear();
                    _budgets.AddRange(document.Budgets
                        .Where(IsValid)
                        .Select(Normalize)
                        .GroupBy(b => (b.Category, b.Month))
                        .Select(g => g.First()));
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

    public IReadOnlyList<DateOnly> AvailableMonths
    {
        get
        {
            var months = _transactions.AvailableMonths.ToHashSet();
            foreach (var budget in _budgets)
            {
                months.Add(budget.Month);
            }

            return months.OrderBy(m => m).ToList();
        }
    }

    public IReadOnlyList<BudgetProgress> GetMonthProgress(DateOnly month)
    {
        var target = BudgetMath.FirstOfMonth(month);
        var spending = _transactions.GetCompletedExpensesByCategory(target);

        return _budgets
            .Where(b => b.Month == target)
            .Select(budget =>
            {
                spending.TryGetValue(budget.Category, out var spent);
                return BudgetMath.Evaluate(budget, spent);
            })
            .OrderByDescending(p => p.PercentUsed)
            .ThenBy(p => p.Budget.Category)
            .ToList();
    }

    public IReadOnlyList<string> AvailableCategories(DateOnly month, string? excludeId = null)
    {
        var target = BudgetMath.FirstOfMonth(month);
        var used = _budgets
            .Where(b => b.Month == target && b.Id != excludeId)
            .Select(b => b.Category)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return FinanceCategories.Expense.Where(category => !used.Contains(category)).ToList();
    }

    public string? Validate(BudgetDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Category) || draft.Limit is null)
        {
            return "Complete all budget fields.";
        }

        if (!FinanceCategories.Expense.Contains(draft.Category))
        {
            return "Choose an expense category.";
        }

        if (BudgetMath.IsDuplicate(_budgets, draft.Category, draft.Month, draft.Id))
        {
            return $"A {draft.Category} budget already exists for {draft.Month:MMMM yyyy}.";
        }

        return null;
    }

    public async Task AddAsync(BudgetDraft draft)
    {
        var error = Validate(draft);
        if (error is not null)
        {
            throw new InvalidOperationException(error);
        }

        _budgets.Add(ToBudget(draft, $"bdg-{Guid.NewGuid():N}"[..16]));
        await PersistAsync();
        Changed?.Invoke();
    }

    public async Task UpdateAsync(BudgetDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Id))
        {
            return;
        }

        var error = Validate(draft);
        if (error is not null)
        {
            throw new InvalidOperationException(error);
        }

        var index = _budgets.FindIndex(b => b.Id == draft.Id);
        if (index < 0)
        {
            return;
        }

        _budgets[index] = ToBudget(draft, draft.Id);
        await PersistAsync();
        Changed?.Invoke();
    }

    public async Task DeleteAsync(string id)
    {
        _budgets.RemoveAll(b => b.Id == id);
        await PersistAsync();
        Changed?.Invoke();
    }

    private void Seed()
    {
        _budgets.Clear();
        _budgets.AddRange(DemoBudgetSeed.Create());
    }

    private async Task PersistAsync()
    {
        var document = new BudgetStoreDocument
        {
            Version = 1,
            Budgets = _budgets.Select(b => b.Clone()).ToList()
        };

        await _js.InvokeVoidAsync("finora.storageSet", StorageKey, JsonSerializer.Serialize(document, JsonOptions));
    }

    private static bool IsValid(CategoryBudget budget) =>
        !string.IsNullOrWhiteSpace(budget.Id)
        && !string.IsNullOrWhiteSpace(budget.Category)
        && budget.Limit > 0;

    private static CategoryBudget Normalize(CategoryBudget budget)
    {
        budget.Month = BudgetMath.FirstOfMonth(budget.Month);
        budget.Limit = Math.Round(Math.Abs(budget.Limit), 2, MidpointRounding.AwayFromZero);
        budget.Category = budget.Category.Trim();
        return budget;
    }

    private static CategoryBudget ToBudget(BudgetDraft draft, string id) => new()
    {
        Id = id,
        Category = draft.Category.Trim(),
        Month = BudgetMath.FirstOfMonth(draft.Month),
        Limit = Math.Round(draft.Limit!.Value, 2, MidpointRounding.AwayFromZero)
    };
}
