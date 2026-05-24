using ShopERP.Backend.Domain.Common;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Domain.Entities;

public class SalesBill : EntityBase
{
    public string BillNo { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public PaymentType PaymentType { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal TotalGstAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal ProfitAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public List<SalesItem> Items { get; set; } = new();
}


