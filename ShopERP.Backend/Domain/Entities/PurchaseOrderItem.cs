using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class PurchaseOrderItem : EntityBase
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
}


