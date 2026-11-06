namespace finora_dashboard.Models;

public sealed class TrendPoint
{
    public required DateOnly Month { get; init; }
    public required decimal Income { get; init; }
    public required decimal Expenses { get; init; }
}
