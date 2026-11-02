using System.ComponentModel.DataAnnotations;

namespace finora_dashboard.Models;

public sealed class BudgetDraft
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Choose a category.")]
    public string Category { get; set; } = "";

    [Required]
    public DateOnly Month { get; set; }

    [Required(ErrorMessage = "Enter a monthly limit.")]
    [Range(0.01, 9_999_999.99, ErrorMessage = "Limit must be greater than 0.")]
    public decimal? Limit { get; set; }

    public static BudgetDraft NewDraft(DateOnly month) => new()
    {
        Month = new DateOnly(month.Year, month.Month, 1)
    };

    public static BudgetDraft From(CategoryBudget budget) => new()
    {
        Id = budget.Id,
        Category = budget.Category,
        Month = budget.Month,
        Limit = budget.Limit
    };
}
