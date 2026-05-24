using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class StockBatch : EntityBase
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string BatchNo { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal SaleRate { get; set; }
    public decimal Mrp { get; set; }
}


