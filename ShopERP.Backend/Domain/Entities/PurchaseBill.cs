using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class PurchaseBill : EntityBase
{
    public string BillNo { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal RoundOff { get; set; }
    public decimal GrandTotal { get; set; }
    public List<PurchaseItem> Items { get; set; } = new();
}


