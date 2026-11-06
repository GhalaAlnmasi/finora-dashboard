namespace finora_dashboard.Models;

public sealed class MonthlySummary
{
    public required DateOnly Month { get; init; }
    public required decimal TotalBalance { get; init; }
    public required decimal Income { get; init; }
    public required decimal Expenses { get; init; }
    public required decimal Savings { get; init; }
    public decimal? BalanceChangePercent { get; init; }
    public decimal? IncomeChangePercent { get; init; }
    public decimal? ExpenseChangePercent { get; init; }
    public decimal? SavingsChangePercent { get; init; }
}
