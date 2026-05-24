using ShopERP.Backend.Domain.Common;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Domain.Entities;

public class PurchaseOrder : EntityBase
{
    public string OrderNo { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal TotalAmount { get; set; }
    public List<PurchaseOrderItem> Items { get; set; } = new();
}


