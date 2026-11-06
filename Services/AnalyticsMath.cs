using finora_dashboard.Models;

namespace finora_dashboard.Services;

public static class AnalyticsMath
{
    public static DateOnly FirstOfMonth(DateOnly month) => new(month.Year, month.Month, 1);

    public static decimal SavingsRate(decimal income, decimal savings) =>
        income <= 0m
            ? 0m
            : Math.Round(savings / income * 100m, 1, MidpointRounding.AwayFromZero);

    public static decimal? ChangePercent(decimal previous, decimal current)
    {
        if (previous == 0m)
        {
            return current == 0m ? 0m : null;
        }

        return Math.Round((current - previous) / Math.Abs(previous) * 100m, 1, MidpointRounding.AwayFromZero);
    }

    public static IReadOnlyList<Transaction> CompletedInRange(
        IEnumerable<Transaction> transactions,
        DateOnly startInclusive,
        DateOnly endExclusive,
        DateOnly today) =>
        transactions
            .Where(t => t.Date <= today && t.Date >= startInclusive && t.Date < endExclusive)
            .ToList();

    public static (decimal Income, decimal Expenses, decimal Savings) Totals(
        IEnumerable<Transaction> completed,
        string? expenseCategory = null)
    {
        var income = completed.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expenses = completed
            .Where(t => t.Type == TransactionType.Expense
                        && (expenseCategory is null || t.Category == expenseCategory))
            .Sum(t => t.Amount);
        return (income, expenses, income - expenses);
    }

    public static IReadOnlyList<TrendPoint> MonthlyTrend(
        IEnumerable<Transaction> transactions,
        DateOnly endMonth,
        int monthCount,
        DateOnly today,
        string? expenseCategory = null)
    {
        var count = Math.Clamp(monthCount, 1, 12);
        var end = FirstOfMonth(endMonth);
        var start = end.AddMonths(1 - count);
        var points = new List<TrendPoint>(count);

        for (var offset = 0; offset < count; offset++)
        {
            var month = start.AddMonths(offset);
            var slice = CompletedInRange(transactions, month, month.AddMonths(1), today);
            var totals = Totals(slice, expenseCategory);
            points.Add(new TrendPoint
            {
                Month = month,
                Income = totals.Income,
                Expenses = totals.Expenses
            });
        }

        return points;
    }

    public static IReadOnlyList<CategoryTotal> ExpenseBreakdown(
        IEnumerable<Transaction> completed,
        string? expenseCategory = null)
    {
        var expenses = completed
            .Where(t => t.Type == TransactionType.Expense
                        && (expenseCategory is null || t.Category == expenseCategory))
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
            Share = total == 0m ? 0m : g.Amount / total
        }).ToList();
    }

    public static bool AmountsReconcile(IEnumerable<CategoryTotal> breakdown, decimal expenseTotal) =>
        Math.Abs(breakdown.Sum(item => item.Amount) - expenseTotal) < 0.01m;
}
