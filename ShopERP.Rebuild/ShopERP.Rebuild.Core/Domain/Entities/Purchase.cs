namespace ShopERP.Rebuild.Core.Domain.Entities;

public sealed class Purchase : EntityBase
{
    public string BillNo { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Status { get; set; } = "Open";
    public ICollection<PurchaseLine> Lines { get; set; } = new List<PurchaseLine>();
}
