using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;

namespace ShopERP.Backend.Services;

public class SupplierLedgerService(ShopErpDbContext dbContext) : ISupplierLedgerService
{
    public async Task AddPurchaseBillEntryAsync(PurchaseBill bill, CancellationToken ct)
    {
        var lastBalance = await dbContext.SupplierLedgerEntries
            .Where(x => x.SupplierId == bill.SupplierId)
            .OrderByDescending(x => x.Id)
            .Select(x => x.Balance)
            .FirstOrDefaultAsync(ct);

        dbContext.SupplierLedgerEntries.Add(new SupplierLedgerEntry
        {
            SupplierId = bill.SupplierId,
            EntryDate = bill.BillDate,
            VoucherNo = bill.BillNo,
            PurchaseAmount = bill.GrandTotal,
            PaymentAmount = 0,
            Balance = lastBalance + bill.GrandTotal,
            Narration = "Purchase bill booked"
        });
    }
}


