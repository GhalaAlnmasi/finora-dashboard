namespace finora_dashboard.Models;

public sealed class FinanceStoreDocument
{
    public int Version { get; set; } = 1;
    public decimal OpeningBalance { get; set; }
    public List<Transaction> Transactions { get; set; } = [];
}
