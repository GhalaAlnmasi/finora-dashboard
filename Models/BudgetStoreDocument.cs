namespace finora_dashboard.Models;

public sealed class BudgetStoreDocument
{
    public int Version { get; set; } = 1;
    public List<CategoryBudget> Budgets { get; set; } = [];
}
