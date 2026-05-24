using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class SalesItem : EntityBase
{
    public int SalesBillId { get; set; }
    public SalesBill SalesBill { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int StockBatchId { get; set; }
    public StockBatch StockBatch { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Mrp { get; set; }
    public decimal SaleRate { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal GstPercent { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal GstAmount { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal Amount { get; set; }
    public decimal ProfitAmount { get; set; }
}


