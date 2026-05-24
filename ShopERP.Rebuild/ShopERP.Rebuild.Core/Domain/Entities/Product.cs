namespace ShopERP.Rebuild.Core.Domain.Entities;

public sealed class Product : EntityBase
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string BatchNo { get; set; } = string.Empty;
    public DateTime? Expiry { get; set; }
    public decimal Price { get; set; }
    public decimal SaleRate { get; set; }
    public decimal Mrp { get; set; }
    public decimal GstPercent { get; set; }
    public int StockQty { get; set; }
    public int MinStockLevel { get; set; } = 10;
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
}
