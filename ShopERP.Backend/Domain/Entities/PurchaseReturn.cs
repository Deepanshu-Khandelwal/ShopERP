using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class PurchaseReturn : EntityBase
{
    public string ReturnNo { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public DateTime ReturnDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseReturnItem> Items { get; set; } = new();
}


