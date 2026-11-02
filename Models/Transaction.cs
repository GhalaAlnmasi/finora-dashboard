namespace finora_dashboard.Models;

public sealed class Transaction
{
    public string Id { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Note { get; set; }

    public decimal SignedAmount => Type == TransactionType.Income ? Amount : -Amount;

    public Transaction Clone() => new()
    {
        Id = Id,
        Description = Description,
        Category = Category,
        Type = Type,
        Amount = Amount,
        Date = Date,
        Note = Note
    };
}
