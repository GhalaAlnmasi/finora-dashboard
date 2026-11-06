using finora_dashboard.Models;

namespace finora_dashboard.Services;

public static class FinanceCategories
{
    public static readonly IReadOnlyList<string> Expense =
    [
        "Housing", "Groceries", "Dining", "Transport", "Shopping",
        "Health", "Entertainment", "Utilities", "Travel"
    ];

    public static readonly IReadOnlyList<string> Income =
    [
        "Salary", "Freelance", "Other income"
    ];

    public static IReadOnlyList<string> For(TransactionType type) =>
        type == TransactionType.Income ? Income : Expense;

    public static IReadOnlyList<string> All => Income.Concat(Expense).ToList();

    public static string Color(string category) => category switch
    {
        "Salary" or "Freelance" or "Other income" => "#166B57",
        "Housing" => "#166B57",
        "Groceries" => "#3A9B7A",
        "Dining" => "#C4A35A",
        "Transport" => "#2F6B8A",
        "Shopping" => "#8B5E3C",
        "Health" => "#5B7C6A",
        "Entertainment" => "#7A8F6B",
        "Utilities" => "#4E6E62",
        "Travel" => "#1F4E46",
        _ => "#5C6F6A"
    };
}
