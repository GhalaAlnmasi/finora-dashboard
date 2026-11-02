namespace finora_dashboard.Models;

public sealed class CategoryTotal
{
    public required string Category { get; init; }
    public required decimal Amount { get; init; }
    public required string Color { get; init; }
    public decimal Share { get; init; }
}
