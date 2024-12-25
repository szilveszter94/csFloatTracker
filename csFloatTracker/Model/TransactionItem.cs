namespace csFloatTracker.Model;

public class TransactionItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SoldPrice { get; set; }
    public decimal Tax { get; set; }
    public decimal PriceAfterTax { get; set; }
    public decimal Profit { get; set; }
    public float Float { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime SoldDate { get; set; } = DateTime.UtcNow;
    public int CsAccountId { get; set; }
    public CsAccount? CsAccount { get; set; }
}
