using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class CustomerLedgerEntry : EntityBase
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime EntryDate { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public decimal BillAmount { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal Balance { get; set; }
    public string Narration { get; set; } = string.Empty;
}


