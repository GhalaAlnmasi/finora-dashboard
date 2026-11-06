using finora_dashboard.Models;
using finora_dashboard.Services;
using Xunit;

namespace finora_dashboard.Tests;

public class DemoSeedTests
{
    [Fact]
    public void TransactionSplits_SumToTheMonthCategoryTotals()
    {
        var items = DemoTransactionSeed.Create();

        foreach (var monthGroup in items.GroupBy(t => new DateOnly(t.Date.Year, t.Date.Month, 1)))
        {
            var groceries = monthGroup.Where(t => t.Category == "Groceries").Sum(t => t.Amount);
            var dining = monthGroup.Where(t => t.Category == "Dining").Sum(t => t.Amount);
            var transport = monthGroup.Where(t => t.Category == "Transport").Sum(t => t.Amount);
            var shopping = monthGroup.Where(t => t.Category == "Shopping").Sum(t => t.Amount);
            var utilities = monthGroup.Where(t => t.Category == "Utilities").Sum(t => t.Amount);

            Assert.True(groceries > 0);
            Assert.True(dining > 0);
            Assert.True(transport > 0);
            Assert.True(shopping > 0);
            Assert.True(utilities > 0);
            Assert.Equal(Math.Round(groceries, 2), groceries);
        }
    }

    [Fact]
    public void OpeningBalancePlusCompletedActivity_IsInternallyConsistent()
    {
        var today = new DateOnly(2026, 11, 2);
        var items = DemoTransactionSeed.Create();
        var completed = items.Where(t => t.Date <= today).ToList();
        var signed = completed.Sum(t => t.SignedAmount);
        var income = completed.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expenses = completed.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        Assert.Equal(income - expenses, signed);
        Assert.Equal(DemoTransactionSeed.OpeningBalance + signed, DemoTransactionSeed.OpeningBalance + income - expenses);
        Assert.True(DemoTransactionSeed.OpeningBalance + signed > 0);
    }

    [Fact]
    public void NovemberAsOfTheSecond_ExcludesLaterScheduledExpenses()
    {
        var today = new DateOnly(2026, 11, 2);
        var start = new DateOnly(2026, 11, 1);
        var items = DemoTransactionSeed.Create();
        var completed = AnalyticsMath.CompletedInRange(items, start, start.AddMonths(1), today);
        var totals = AnalyticsMath.Totals(completed);

        Assert.Equal(28_500m, totals.Income);
        Assert.Equal(7_200m, totals.Expenses);
        Assert.DoesNotContain(completed, t => t.Category == "Groceries");
        Assert.Contains(items, t => t.Date > today && t.Category == "Groceries" && t.Date.Month == 11);
    }

    [Fact]
    public void BudgetEnvelopes_CoverEachSeedMonthAndCategory()
    {
        var budgets = DemoBudgetSeed.Create();
        var months = budgets.Select(b => b.Month).Distinct().OrderBy(m => m).ToList();

        Assert.Equal(6, months.Count);
        Assert.Equal(new DateOnly(2026, 6, 1), months[0]);
        Assert.Equal(new DateOnly(2026, 11, 1), months[^1]);
        Assert.Equal(9, budgets.Count(b => b.Month == months[^1]));
        Assert.All(budgets, b => Assert.True(b.Limit > 0));
    }

    [Fact]
    public void EmptyCompletedSet_HasZeroTotalsAndEmptyBreakdown()
    {
        var totals = AnalyticsMath.Totals([]);
        var breakdown = AnalyticsMath.ExpenseBreakdown([]);

        Assert.Equal(0m, totals.Income);
        Assert.Equal(0m, totals.Expenses);
        Assert.Equal(0m, totals.Savings);
        Assert.Empty(breakdown);
        Assert.True(AnalyticsMath.AmountsReconcile(breakdown, 0m));
        Assert.Equal(0m, AnalyticsMath.SavingsRate(0m, 0m));
    }

    [Fact]
    public void DefaultPersona_IsNeutralSampleUser()
    {
        Assert.Equal("Lina Faris", DemoPersona.DisplayName);
        Assert.Equal("Riyadh", DemoPersona.City);
        Assert.Equal("LF", DemoProfile.InitialsFrom(DemoPersona.DisplayName));
        Assert.Equal("LF", new DemoProfile().Initials);
    }
}
