using finora_dashboard.Models;
using finora_dashboard.Services;
using Xunit;

namespace finora_dashboard.Tests;

public class AnalyticsMathTests
{
    private static readonly DateOnly Today = new(2026, 11, 2);

    [Fact]
    public void SavingsRate_IsZero_WhenIncomeIsZero()
    {
        Assert.Equal(0m, AnalyticsMath.SavingsRate(0m, 0m));
        Assert.Equal(0m, AnalyticsMath.SavingsRate(0m, -400m));
    }

    [Fact]
    public void SavingsRate_UsesIncomeAsDenominator()
    {
        Assert.Equal(40m, AnalyticsMath.SavingsRate(10_000m, 4_000m));
    }

    [Fact]
    public void CompletedInRange_ExcludesFutureTransactions()
    {
        var items = new[]
        {
            Tx("past", TransactionType.Expense, 100m, new DateOnly(2026, 11, 1)),
            Tx("future", TransactionType.Expense, 900m, new DateOnly(2026, 11, 20))
        };

        var completed = AnalyticsMath.CompletedInRange(items, new DateOnly(2026, 11, 1), new DateOnly(2026, 12, 1), Today);

        Assert.Single(completed);
        Assert.Equal("past", completed[0].Description);
        Assert.Equal(100m, AnalyticsMath.Totals(completed).Expenses);
    }

    [Fact]
    public void MonthlyTrend_RespectsLookbackWindow()
    {
        var items = new[]
        {
            Tx("jun", TransactionType.Expense, 10m, new DateOnly(2026, 6, 10)),
            Tx("oct", TransactionType.Expense, 40m, new DateOnly(2026, 10, 10)),
            Tx("nov", TransactionType.Expense, 50m, new DateOnly(2026, 11, 1))
        };

        var six = AnalyticsMath.MonthlyTrend(items, new DateOnly(2026, 11, 1), 6, Today);
        var twelve = AnalyticsMath.MonthlyTrend(items, new DateOnly(2026, 11, 1), 12, Today);

        Assert.Equal(6, six.Count);
        Assert.Equal(new DateOnly(2026, 6, 1), six[0].Month);
        Assert.Equal(new DateOnly(2026, 11, 1), six[^1].Month);
        Assert.Equal(10m, six[0].Expenses);
        Assert.Equal(12, twelve.Count);
        Assert.Equal(new DateOnly(2025, 12, 1), twelve[0].Month);
        Assert.Equal(new DateOnly(2026, 11, 1), twelve[^1].Month);
        Assert.Equal(0m, twelve[0].Expenses);
    }

    [Fact]
    public void ChangePercent_ComparesMonthOverMonth()
    {
        Assert.Equal(50m, AnalyticsMath.ChangePercent(100m, 150m));
        Assert.Equal(-20m, AnalyticsMath.ChangePercent(50m, 40m));
        Assert.Equal(0m, AnalyticsMath.ChangePercent(0m, 0m));
        Assert.Null(AnalyticsMath.ChangePercent(0m, 25m));
    }

    [Fact]
    public void ExpenseBreakdown_ReconcilesWithExpenseTotal()
    {
        var items = new[]
        {
            Tx("rent", TransactionType.Expense, 7_200m, new DateOnly(2026, 11, 1), "Housing"),
            Tx("food", TransactionType.Expense, 800m, new DateOnly(2026, 11, 1), "Groceries"),
            Tx("pay", TransactionType.Income, 28_500m, new DateOnly(2026, 11, 1), "Salary")
        };

        var totals = AnalyticsMath.Totals(items);
        var breakdown = AnalyticsMath.ExpenseBreakdown(items);

        Assert.Equal(8_000m, totals.Expenses);
        Assert.Equal(20_500m, totals.Savings);
        Assert.True(AnalyticsMath.AmountsReconcile(breakdown, totals.Expenses));
        Assert.Equal(1m, breakdown.Sum(item => item.Share));
    }

    [Fact]
    public void Totals_CategoryFilter_DoesNotChangeIncome()
    {
        var items = new[]
        {
            Tx("pay", TransactionType.Income, 1_000m, new DateOnly(2026, 11, 1), "Salary"),
            Tx("rent", TransactionType.Expense, 400m, new DateOnly(2026, 11, 1), "Housing"),
            Tx("food", TransactionType.Expense, 100m, new DateOnly(2026, 11, 1), "Groceries")
        };

        var filtered = AnalyticsMath.Totals(items, "Housing");
        Assert.Equal(1_000m, filtered.Income);
        Assert.Equal(400m, filtered.Expenses);
        Assert.Equal(600m, filtered.Savings);
    }

    private static Transaction Tx(string name, TransactionType type, decimal amount, DateOnly date, string? category = null) => new()
    {
        Id = name,
        Description = name,
        Category = category ?? (type == TransactionType.Income ? "Salary" : "Shopping"),
        Type = type,
        Amount = amount,
        Date = date
    };
}
