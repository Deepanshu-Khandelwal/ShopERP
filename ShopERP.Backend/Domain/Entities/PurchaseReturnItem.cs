using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class PurchaseReturnItem : EntityBase
{
    public int PurchaseReturnId { get; set; }
    public PurchaseReturn PurchaseReturn { get; set; } = null!;
    public int ProductId { get; set; }
    public int StockBatchId { get; set; }
    public int Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
}


