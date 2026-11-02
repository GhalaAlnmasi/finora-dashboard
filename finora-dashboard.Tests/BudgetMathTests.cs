using finora_dashboard.Models;
using finora_dashboard.Services;
using Xunit;

namespace finora_dashboard.Tests;

public class BudgetMathTests
{
    [Fact]
    public void Evaluate_OnTrack_WhenUsageIsBelowWarning()
    {
        var result = BudgetMath.Evaluate(Budget("Groceries", 1_000m), 200m);

        Assert.Equal(BudgetHealth.OnTrack, result.Health);
        Assert.Equal(800m, result.Remaining);
        Assert.Equal(20m, result.PercentUsed);
        Assert.Equal(20d, BudgetMath.MeterPercent(result.PercentUsed));
    }

    [Fact]
    public void Evaluate_Warning_WhenUsageReachesEightyPercent()
    {
        var result = BudgetMath.Evaluate(Budget("Dining", 1_000m), 800m);

        Assert.Equal(BudgetHealth.Warning, result.Health);
        Assert.Equal(200m, result.Remaining);
        Assert.Contains("approaching limit", result.StatusLabel);
    }

    [Fact]
    public void Evaluate_Over_WhenSpendingExceedsLimit()
    {
        var result = BudgetMath.Evaluate(Budget("Shopping", 1_000m), 1_250m);

        Assert.Equal(BudgetHealth.Over, result.Health);
        Assert.Equal(-250m, result.Remaining);
        Assert.Equal(125m, result.PercentUsed);
        Assert.Equal(100d, BudgetMath.MeterPercent(result.PercentUsed));
        Assert.Contains("Over budget", result.StatusLabel);
        Assert.Contains("SAR", result.StatusLabel);
    }

    [Fact]
    public void IsDuplicate_DetectsSameCategoryAndMonth()
    {
        var month = new DateOnly(2026, 11, 1);
        var budgets = new[]
        {
            new CategoryBudget { Id = "a", Category = "Housing", Month = month, Limit = 7_200m }
        };

        Assert.True(BudgetMath.IsDuplicate(budgets, "housing", new DateOnly(2026, 11, 18)));
        Assert.False(BudgetMath.IsDuplicate(budgets, "Housing", month, "a"));
        Assert.False(BudgetMath.IsDuplicate(budgets, "Groceries", month));
    }

    private static CategoryBudget Budget(string category, decimal limit) => new()
    {
        Id = "bdg-test",
        Category = category,
        Month = new DateOnly(2026, 11, 1),
        Limit = limit
    };
}
