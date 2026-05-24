namespace ShopERP.Rebuild.Core.Domain.Entities;

public sealed class SaleLine : EntityBase
{
    public Guid SaleId { get; set; }
    public Sale? Sale { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal => (Quantity * UnitPrice) + TaxAmount - DiscountAmount;
}
