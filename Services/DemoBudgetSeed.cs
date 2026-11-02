using finora_dashboard.Models;

namespace finora_dashboard.Services;

public static class DemoBudgetSeed
{
    public static List<CategoryBudget> Create()
    {
        var months = Enumerable.Range(6, 6)
            .Select(month => new DateOnly(2026, month, 1))
            .ToList();

        var limits = new Dictionary<string, decimal>
        {
            ["Housing"] = 7_200m,
            ["Groceries"] = 2_100m,
            ["Dining"] = 900m,
            ["Transport"] = 650m,
            ["Shopping"] = 1_400m,
            ["Health"] = 400m,
            ["Entertainment"] = 450m,
            ["Utilities"] = 800m,
            ["Travel"] = 2_500m
        };

        var items = new List<CategoryBudget>();
        var n = 1;
        foreach (var month in months)
        {
            foreach (var (category, limit) in limits)
            {
                items.Add(new CategoryBudget
                {
                    Id = $"bdg-{n:000}",
                    Category = category,
                    Month = month,
                    Limit = limit
                });
                n++;
            }
        }

        return items;
    }
}
