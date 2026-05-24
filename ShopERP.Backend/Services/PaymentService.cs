using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Services;

public class PaymentService(ShopErpDbContext dbContext) : IPaymentService
{
    public async Task<PaymentEntry> RecordPaymentAsync(PaymentCreateRequest request, CancellationToken ct)
    {
        var payment = new PaymentEntry
        {
            PartyType = request.PartyType,
            PartyId = request.PartyId,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate,
            ReferenceNo = request.ReferenceNo,
            Notes = request.Notes
        };
        dbContext.PaymentEntries.Add(payment);

        if (request.PartyType == PaymentPartyType.Customer)
        {
            var balance = await dbContext.CustomerLedgerEntries
                .Where(x => x.CustomerId == request.PartyId)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Balance)
                .FirstOrDefaultAsync(ct);

            dbContext.CustomerLedgerEntries.Add(new CustomerLedgerEntry
            {
                CustomerId = request.PartyId,
                EntryDate = request.PaymentDate,
                VoucherNo = request.ReferenceNo,
                BillAmount = 0,
                PaymentAmount = request.Amount,
                Balance = balance - request.Amount,
                Narration = "Customer payment"
            });
        }
        else
        {
            var balance = await dbContext.SupplierLedgerEntries
                .Where(x => x.SupplierId == request.PartyId)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Balance)
                .FirstOrDefaultAsync(ct);

            dbContext.SupplierLedgerEntries.Add(new SupplierLedgerEntry
            {
                SupplierId = request.PartyId,
                EntryDate = request.PaymentDate,
                VoucherNo = request.ReferenceNo,
                PurchaseAmount = 0,
                PaymentAmount = request.Amount,
                Balance = balance - request.Amount,
                Narration = "Supplier payment"
            });
        }

        await dbContext.SaveChangesAsync(ct);
        return payment;
    }
}


