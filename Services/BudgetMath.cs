using finora_dashboard.Models;

namespace finora_dashboard.Services;

public static class BudgetMath
{
    public const decimal WarningThreshold = 80m;

    public static BudgetProgress Evaluate(CategoryBudget budget, decimal spent)
    {
        var safeSpent = Math.Max(0m, spent);
        var remaining = Math.Round(budget.Limit - safeSpent, 2, MidpointRounding.AwayFromZero);
        var percent = budget.Limit <= 0
            ? 0
            : Math.Round(safeSpent / budget.Limit * 100m, 1, MidpointRounding.AwayFromZero);

        var health = safeSpent > budget.Limit
            ? BudgetHealth.Over
            : budget.Limit > 0 && safeSpent == budget.Limit
                ? BudgetHealth.AtLimit
                : percent >= WarningThreshold
                    ? BudgetHealth.Warning
                    : BudgetHealth.OnTrack;

        var status = health switch
        {
            BudgetHealth.Over => $"Over budget by {Money.Sar(Math.Abs(remaining))}",
            BudgetHealth.AtLimit => "At limit · 100% used",
            BudgetHealth.Warning => $"{percent:0.#}% used · approaching limit",
            _ => remaining == budget.Limit ? "No completed spending yet" : $"{percent:0.#}% used"
        };

        return new BudgetProgress
        {
            Budget = budget,
            Spent = safeSpent,
            Remaining = remaining,
            PercentUsed = percent,
            Health = health,
            StatusLabel = status
        };
    }

    public static double MeterPercent(decimal percentUsed) =>
        (double)Math.Clamp(percentUsed, 0m, 100m);

    public static bool IsDuplicate(
        IEnumerable<CategoryBudget> budgets,
        string category,
        DateOnly month,
        string? excludeId = null)
    {
        var target = FirstOfMonth(month);
        return budgets.Any(budget =>
            budget.Id != excludeId
            && budget.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
            && budget.Month == target);
    }

    public static DateOnly FirstOfMonth(DateOnly month) => new(month.Year, month.Month, 1);
}
