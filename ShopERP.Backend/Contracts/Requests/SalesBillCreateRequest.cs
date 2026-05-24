using ShopERP.Backend.Contracts.Dtos;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Contracts.Requests;

public class SalesBillCreateRequest
{
    public string BillNo { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public PaymentType PaymentType { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerMobile { get; set; }
    public string? CustomerAddress { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal PaidAmount { get; set; }
    public List<BillLineDto> Items { get; set; } = new();
}


