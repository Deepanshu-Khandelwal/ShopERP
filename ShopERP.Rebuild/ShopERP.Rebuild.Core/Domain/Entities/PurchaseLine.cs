namespace ShopERP.Rebuild.Core.Domain.Entities;

public sealed class PurchaseLine : EntityBase
{
    public Guid PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public decimal Mrp { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal => (Quantity * UnitPrice) + TaxAmount - DiscountAmount;
}
