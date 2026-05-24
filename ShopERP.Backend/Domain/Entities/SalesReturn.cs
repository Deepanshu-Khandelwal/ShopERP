using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class SalesReturn : EntityBase
{
    public string ReturnNo { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime ReturnDate { get; set; }
    public decimal RefundAmount { get; set; }
    public List<SalesReturnItem> Items { get; set; } = new();
}


