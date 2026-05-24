namespace ShopERP.Rebuild.Core.Domain.Entities;

public sealed class Sale : EntityBase
{
    public string InvoiceNo { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal Total { get; set; }
    public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();
}
