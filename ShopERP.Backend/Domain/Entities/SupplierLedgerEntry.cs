using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class SupplierLedgerEntry : EntityBase
{
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public DateTime EntryDate { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public decimal PurchaseAmount { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal Balance { get; set; }
    public string Narration { get; set; } = string.Empty;
}


