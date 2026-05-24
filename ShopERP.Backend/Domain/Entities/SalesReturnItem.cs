using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class SalesReturnItem : EntityBase
{
    public int SalesReturnId { get; set; }
    public SalesReturn SalesReturn { get; set; } = null!;
    public int ProductId { get; set; }
    public int StockBatchId { get; set; }
    public int Quantity { get; set; }
    public decimal SaleRate { get; set; }
    public decimal Amount { get; set; }
}


