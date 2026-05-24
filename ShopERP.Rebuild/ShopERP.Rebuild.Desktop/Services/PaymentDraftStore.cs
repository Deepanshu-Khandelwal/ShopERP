using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Rebuild.Desktop.Services;

public sealed record PaymentDraft(
    int PaymentId,
    PaymentPartyType OriginalPartyType,
    int OriginalPartyId,
    string OriginalReferenceNo,
    PaymentPartyType PartyType,
    int PartyId,
    decimal Amount,
    DateTime PaymentDate,
    string ReferenceNo,
    string Notes);

public sealed class PaymentDraftStore
{
    public PaymentDraft? Current { get; private set; }

    public void SetFrom(PaymentEntry payment)
    {
        Current = new PaymentDraft(
            payment.Id,
            payment.PartyType,
            payment.PartyId,
            payment.ReferenceNo,
            payment.PartyType,
            payment.PartyId,
            payment.Amount,
            payment.PaymentDate,
            payment.ReferenceNo,
            payment.Notes);
    }

    public void Clear()
    {
        Current = null;
    }
}