namespace csFloatTracker.Model;

public class InventoryItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public float Float { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public int CsAccountId { get; set; }
    public CsAccount? CsAccount { get; set; }
}
