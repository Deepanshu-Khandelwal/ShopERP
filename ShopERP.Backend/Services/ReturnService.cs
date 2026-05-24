using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Services;

public class ReturnService(ShopErpDbContext dbContext, IStockService stockService) : IReturnService
{
    public async Task<PurchaseReturn> CreatePurchaseReturnAsync(ReturnCreateRequest request, CancellationToken ct)
    {
        if (!request.SupplierId.HasValue)
        {
            throw new InvalidOperationException("Supplier is required for purchase return.");
        }

        var result = new PurchaseReturn
        {
            ReturnNo = request.ReturnNo,
            SupplierId = request.SupplierId.Value,
            ReturnDate = request.ReturnDate
        };

        foreach (var line in request.Items)
        {
            var batch = await dbContext.StockBatches.FirstOrDefaultAsync(x => x.Id == line.StockBatchId, ct)
                ?? throw new InvalidOperationException("Batch not found.");

            await stockService.DecreaseAsync(line.StockBatchId, line.Quantity, request.ReturnNo, ct);

            result.Items.Add(new PurchaseReturnItem
            {
                ProductId = line.ProductId,
                StockBatchId = line.StockBatchId,
                Quantity = line.Quantity,
                Rate = line.Rate,
                Amount = Math.Round(line.Quantity * line.Rate, 2)
            });
        }

        result.TotalAmount = result.Items.Sum(x => x.Amount);
        dbContext.PurchaseReturns.Add(result);

        var supplierBalance = await dbContext.SupplierLedgerEntries
            .Where(x => x.SupplierId == request.SupplierId.Value)
            .OrderByDescending(x => x.Id)
            .Select(x => x.Balance)
            .FirstOrDefaultAsync(ct);

        dbContext.SupplierLedgerEntries.Add(new SupplierLedgerEntry
        {
            SupplierId = request.SupplierId.Value,
            EntryDate = request.ReturnDate,
            VoucherNo = request.ReturnNo,
            PurchaseAmount = -result.TotalAmount,
            PaymentAmount = 0,
            Balance = supplierBalance - result.TotalAmount,
            Narration = "Purchase return"
        });

        await dbContext.SaveChangesAsync(ct);
        return result;
    }

    public async Task<SalesReturn> CreateSalesReturnAsync(ReturnCreateRequest request, CancellationToken ct)
    {
        var result = new SalesReturn
        {
            ReturnNo = request.ReturnNo,
            CustomerId = request.CustomerId,
            ReturnDate = request.ReturnDate
        };

        foreach (var line in request.Items)
        {
            var batch = await dbContext.StockBatches.FirstOrDefaultAsync(x => x.Id == line.StockBatchId, ct)
                ?? throw new InvalidOperationException("Batch not found.");

            await stockService.IncreaseAsync(line.ProductId, batch.BatchNo, batch.ExpiryDate, line.Quantity, batch.PurchaseRate, batch.SaleRate, batch.Mrp, StockMovementType.SalesReturn, request.ReturnNo, ct);

            result.Items.Add(new SalesReturnItem
            {
                ProductId = line.ProductId,
                StockBatchId = line.StockBatchId,
                Quantity = line.Quantity,
                SaleRate = line.Rate,
                Amount = Math.Round(line.Quantity * line.Rate, 2)
            });
        }

        result.RefundAmount = result.Items.Sum(x => x.Amount);
        dbContext.SalesReturns.Add(result);

        if (request.CustomerId.HasValue)
        {
            var customerBalance = await dbContext.CustomerLedgerEntries
                .Where(x => x.CustomerId == request.CustomerId.Value)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Balance)
                .FirstOrDefaultAsync(ct);

            dbContext.CustomerLedgerEntries.Add(new CustomerLedgerEntry
            {
                CustomerId = request.CustomerId.Value,
                EntryDate = request.ReturnDate,
                VoucherNo = request.ReturnNo,
                BillAmount = -result.RefundAmount,
                PaymentAmount = 0,
                Balance = customerBalance - result.RefundAmount,
                Narration = "Sales return"
            });
        }

        await dbContext.SaveChangesAsync(ct);
        return result;
    }
}


