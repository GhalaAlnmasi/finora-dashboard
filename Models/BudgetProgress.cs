namespace finora_dashboard.Models;

public sealed class BudgetProgress
{
    public required CategoryBudget Budget { get; init; }
    public required decimal Spent { get; init; }
    public required decimal Remaining { get; init; }
    public required decimal PercentUsed { get; init; }
    public required BudgetHealth Health { get; init; }
    public required string StatusLabel { get; init; }
}
