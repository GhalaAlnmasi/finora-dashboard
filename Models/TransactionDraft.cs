using System.ComponentModel.DataAnnotations;

namespace finora_dashboard.Models;

public sealed class TransactionDraft
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Enter a description.")]
    [MaxLength(80, ErrorMessage = "Description must be 80 characters or fewer.")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Enter an amount.")]
    [Range(0.01, 9_999_999.99, ErrorMessage = "Amount must be greater than 0.")]
    public decimal? Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required(ErrorMessage = "Choose a category.")]
    public string Category { get; set; } = "";

    [Required(ErrorMessage = "Choose a date.")]
    public DateOnly? Date { get; set; }

    public static TransactionDraft From(Transaction transaction) => new()
    {
        Id = transaction.Id,
        Description = transaction.Description,
        Amount = transaction.Amount,
        Type = transaction.Type,
        Category = transaction.Category,
        Date = transaction.Date
    };

    public static TransactionDraft NewDraft(DateOnly today) => new()
    {
        Type = TransactionType.Expense,
        Date = today
    };
}
