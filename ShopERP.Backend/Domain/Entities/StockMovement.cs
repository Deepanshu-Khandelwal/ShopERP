using ShopERP.Backend.Domain.Common;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Domain.Entities;

public class StockMovement : EntityBase
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? StockBatchId { get; set; }
    public StockBatch? StockBatch { get; set; }
    public StockMovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
}


