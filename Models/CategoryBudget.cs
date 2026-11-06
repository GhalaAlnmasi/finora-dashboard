namespace finora_dashboard.Models;

public sealed class CategoryBudget
{
    public string Id { get; set; } = "";
    public string Category { get; set; } = "";
    public DateOnly Month { get; set; }
    public decimal Limit { get; set; }

    public CategoryBudget Clone() => new()
    {
        Id = Id,
        Category = Category,
        Month = Month,
        Limit = Limit
    };
}
