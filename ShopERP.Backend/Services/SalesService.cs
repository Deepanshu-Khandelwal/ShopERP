using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;

namespace ShopERP.Backend.Services;

public class SalesService(
    ShopErpDbContext dbContext,
    IStockService stockService,
    ICustomerService customerService,
    ISalesCalculationService salesCalculationService,
    ILedgerService ledgerService) : ISalesService
{
    public async Task<SalesBill> CreateAsync(SalesBillCreateRequest request, CancellationToken ct)
    {
        var customerId = await customerService.ResolveOrCreateCustomerIdAsync(
            request.CustomerId,
            request.CustomerName,
            request.CustomerMobile,
            request.CustomerAddress,
            ct);
        var doctorId = await customerService.ResolveOrCreateDoctorIdAsync(request.DoctorId, request.DoctorName, ct);

        var bill = new SalesBill
        {
            BillNo = request.BillNo,
            BillDate = request.BillDate,
            PaymentType = request.PaymentType,
            CustomerId = customerId,
            DoctorId = doctorId,
            DiscountPercent = request.DiscountPercent
        };

        foreach (var item in request.Items)
        {
            if (item.StockBatchId is null)
            {
                throw new InvalidOperationException("Batch selection is required for each sales item.");
            }

            var batch = await dbContext.StockBatches.FirstOrDefaultAsync(x => x.Id == item.StockBatchId.Value, ct)
                ?? throw new InvalidOperationException("Selected batch not found.");

            bill.Items.Add(salesCalculationService.BuildSalesItem(batch, item));

            await stockService.DecreaseAsync(batch.Id, item.Quantity, request.BillNo, ct);
        }

        salesCalculationService.ApplyBillTotals(bill, request.PaidAmount);

        dbContext.SalesBills.Add(bill);
        await ledgerService.AddCustomerSalesEntryAsync(bill, ct);

        await dbContext.SaveChangesAsync(ct);
        return bill;
    }
}


