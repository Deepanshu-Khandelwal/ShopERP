using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Contracts.Requests;

public class PaymentCreateRequest
{
    public PaymentPartyType PartyType { get; set; }
    public int PartyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}


