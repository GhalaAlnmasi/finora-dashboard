using finora_dashboard.Models;

namespace finora_dashboard.Services;

public static class DemoTransactionSeed
{
    public const decimal OpeningBalance = 62_400m;

    public static List<Transaction> Create()
    {
        var items = new List<Transaction>();
        var months = new[]
        {
            (Year: 2026, Month: 6, Salary: 28_500m, Rent: 7_200m, Groceries: 2_140m, Dining: 890m, Transport: 640m, Shopping: 1_180m, Health: 320m, Entertainment: 410m, Utilities: 780m, Travel: 0m, ExtraIncome: 0m),
            (Year: 2026, Month: 7, Salary: 28_500m, Rent: 7_200m, Groceries: 2_280m, Dining: 1_040m, Transport: 710m, Shopping: 860m, Health: 0m, Entertainment: 540m, Utilities: 810m, Travel: 3_200m, ExtraIncome: 1_200m),
            (Year: 2026, Month: 8, Salary: 28_500m, Rent: 7_200m, Groceries: 2_050m, Dining: 760m, Transport: 590m, Shopping: 1_540m, Health: 480m, Entertainment: 390m, Utilities: 795m, Travel: 0m, ExtraIncome: 0m),
            (Year: 2026, Month: 9, Salary: 28_500m, Rent: 7_200m, Groceries: 2_310m, Dining: 1_120m, Transport: 680m, Shopping: 2_080m, Health: 210m, Entertainment: 670m, Utilities: 770m, Travel: 0m, ExtraIncome: 2_400m),
            (Year: 2026, Month: 10, Salary: 28_500m, Rent: 7_200m, Groceries: 2_190m, Dining: 980m, Transport: 650m, Shopping: 1_260m, Health: 360m, Entertainment: 450m, Utilities: 805m, Travel: 0m, ExtraIncome: 0m),
            (Year: 2026, Month: 11, Salary: 28_500m, Rent: 7_200m, Groceries: 1_860m, Dining: 640m, Transport: 420m, Shopping: 740m, Health: 0m, Entertainment: 280m, Utilities: 760m, Travel: 0m, ExtraIncome: 0m)
        };

        var id = 1;
        foreach (var month in months)
        {
            Add(items, ref id, "Al Rajhi Payroll", "Salary", TransactionType.Income, month.Salary, new DateOnly(month.Year, month.Month, 1), "Monthly salary");
            if (month.ExtraIncome > 0)
            {
                Add(items, ref id, "Design retainer", "Freelance", TransactionType.Income, month.ExtraIncome, new DateOnly(month.Year, month.Month, 18), "Freelance project");
            }

            Add(items, ref id, "Olaya residence", "Housing", TransactionType.Expense, month.Rent, new DateOnly(month.Year, month.Month, 2), "Rent");
            Add(items, ref id, "Tamimi Markets", "Groceries", TransactionType.Expense, Math.Round(month.Groceries * 0.58m, 2), new DateOnly(month.Year, month.Month, 6));
            Add(items, ref id, "Danube", "Groceries", TransactionType.Expense, month.Groceries - Math.Round(month.Groceries * 0.58m, 2), new DateOnly(month.Year, month.Month, 20));
            Add(items, ref id, "Café Central", "Dining", TransactionType.Expense, Math.Round(month.Dining * 0.45m, 2), new DateOnly(month.Year, month.Month, 8));
            Add(items, ref id, "Lusin", "Dining", TransactionType.Expense, month.Dining - Math.Round(month.Dining * 0.45m, 2), new DateOnly(month.Year, month.Month, 22));
            Add(items, ref id, "STC", "Utilities", TransactionType.Expense, Math.Round(month.Utilities * 0.42m, 2), new DateOnly(month.Year, month.Month, 11), "Mobile & fiber");
            Add(items, ref id, "SEC electricity", "Utilities", TransactionType.Expense, month.Utilities - Math.Round(month.Utilities * 0.42m, 2), new DateOnly(month.Year, month.Month, 14));
            Add(items, ref id, "Uber", "Transport", TransactionType.Expense, Math.Round(month.Transport * 0.4m, 2), new DateOnly(month.Year, month.Month, 9));
            Add(items, ref id, "Petrol — SASCO", "Transport", TransactionType.Expense, month.Transport - Math.Round(month.Transport * 0.4m, 2), new DateOnly(month.Year, month.Month, 16));
            Add(items, ref id, "Jarir Bookstore", "Shopping", TransactionType.Expense, Math.Round(month.Shopping * 0.35m, 2), new DateOnly(month.Year, month.Month, 12));
            Add(items, ref id, "Namshi", "Shopping", TransactionType.Expense, month.Shopping - Math.Round(month.Shopping * 0.35m, 2), new DateOnly(month.Year, month.Month, 25));

            if (month.Health > 0)
            {
                Add(items, ref id, "Dr. Sulaiman Al Habib", "Health", TransactionType.Expense, month.Health, new DateOnly(month.Year, month.Month, 15));
            }

            if (month.Entertainment > 0)
            {
                Add(items, ref id, "VOX Cinemas", "Entertainment", TransactionType.Expense, month.Entertainment, new DateOnly(month.Year, month.Month, 19));
            }

            if (month.Travel > 0)
            {
                Add(items, ref id, "Saudia", "Travel", TransactionType.Expense, month.Travel, new DateOnly(month.Year, month.Month, 27), "Jeddah weekend");
            }
        }

        return items;
    }

    private static void Add(
        List<Transaction> items,
        ref int id,
        string description,
        string category,
        TransactionType type,
        decimal amount,
        DateOnly date,
        string? note = null)
    {
        if (amount <= 0)
        {
            return;
        }

        items.Add(new Transaction
        {
            Id = $"txn-{id:000}",
            Description = description,
            Category = category,
            Type = type,
            Amount = amount,
            Date = date,
            Note = note
        });
        id++;
    }
}
