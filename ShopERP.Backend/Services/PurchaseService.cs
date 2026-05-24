using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Services;

public class PurchaseService(
    ShopErpDbContext dbContext,
    IStockService stockService,
    IPurchaseCalculationService purchaseCalculationService,
    ISupplierService supplierService,
    ISupplierLedgerService supplierLedgerService) : IPurchaseService
{
    public async Task<PurchaseBill> CreateAsync(PurchaseBillCreateRequest request, CancellationToken ct)
    {
        var supplier = await supplierService.GetSupplierOrThrowAsync(request.SupplierId, ct);

        var bill = new PurchaseBill
        {
            BillNo = request.BillNo,
            SupplierId = supplier.Id,
            BillDate = request.BillDate,
            RoundOff = request.RoundOff
        };

        decimal subtotal = 0;
        decimal cgst = 0;
        decimal sgst = 0;

        foreach (var item in request.Items)
        {
            var line = purchaseCalculationService.BuildPurchaseItem(item);
            subtotal += line.Taxable;
            cgst += line.Cgst;
            sgst += line.Sgst;

            bill.Items.Add(line.Item);

            await stockService.IncreaseAsync(item.ProductId, item.BatchNo, item.ExpiryDate ?? DateTime.UtcNow.AddYears(1), item.Quantity, item.Rate, item.Rate, item.Mrp, StockMovementType.Purchase, request.BillNo, ct);
        }

        purchaseCalculationService.ApplyBillTotals(bill, request.LessDiscount, request.RoundOff, subtotal, cgst, sgst);

        dbContext.PurchaseBills.Add(bill);
        await supplierLedgerService.AddPurchaseBillEntryAsync(bill, ct);

        await dbContext.SaveChangesAsync(ct);
        return bill;
    }
}


