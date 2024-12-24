namespace csFloatTracker.Model;

public class CsAccount
{
    public int Id { get; set; }
    public int SoldCount { get; set; }
    public int PurchasedCount { get; set; }
    public decimal Balance { get; set; }
    public decimal Profit { get; set; }
    public ICollection<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
    public ICollection<TransactionItem> TransactionHistory { get; set; } = new List<TransactionItem>();
}
