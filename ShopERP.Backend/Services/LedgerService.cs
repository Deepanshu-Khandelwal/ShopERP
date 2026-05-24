using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;

namespace ShopERP.Backend.Services;

public class LedgerService(ShopErpDbContext dbContext) : ILedgerService
{
    public async Task AddCustomerSalesEntryAsync(SalesBill bill, CancellationToken ct)
    {
        if (!bill.CustomerId.HasValue)
        {
            return;
        }

        var customerBalance = await dbContext.CustomerLedgerEntries
            .Where(x => x.CustomerId == bill.CustomerId.Value)
            .OrderByDescending(x => x.Id)
            .Select(x => x.Balance)
            .FirstOrDefaultAsync(ct);

        dbContext.CustomerLedgerEntries.Add(new CustomerLedgerEntry
        {
            CustomerId = bill.CustomerId.Value,
            EntryDate = bill.BillDate,
            VoucherNo = bill.BillNo,
            BillAmount = bill.GrandTotal,
            PaymentAmount = bill.PaidAmount,
            Balance = customerBalance + bill.DueAmount,
            Narration = "Sales bill booked"
        });
    }
}


