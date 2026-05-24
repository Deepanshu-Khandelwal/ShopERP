using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class PurchaseItem : EntityBase
{
    public int PurchaseBillId { get; set; }
    public PurchaseBill PurchaseBill { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string BatchNo { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Mrp { get; set; }
    public decimal Rate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal GstPercent { get; set; }
    public decimal Amount { get; set; }
}


