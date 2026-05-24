using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Services;

public class StockService(ShopErpDbContext dbContext) : IStockService
{
    public async Task IncreaseAsync(int productId, string batchNo, DateTime expiryDate, int quantity, decimal purchaseRate, decimal saleRate, decimal mrp, StockMovementType movementType, string referenceNo, CancellationToken ct)
    {
        var batch = await dbContext.StockBatches.FirstOrDefaultAsync(x => x.ProductId == productId && x.BatchNo == batchNo, ct);
        if (batch is null)
        {
            batch = new StockBatch
            {
                ProductId = productId,
                BatchNo = batchNo,
                ExpiryDate = expiryDate,
                Quantity = 0,
                PurchaseRate = purchaseRate,
                SaleRate = saleRate,
                Mrp = mrp
            };
            dbContext.StockBatches.Add(batch);
        }

        batch.Quantity += quantity;
        batch.PurchaseRate = purchaseRate;
        batch.SaleRate = saleRate;
        batch.Mrp = mrp;
        batch.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.StockMovements.Add(new StockMovement
        {
            ProductId = productId,
            StockBatch = batch,
            MovementType = movementType,
            Quantity = quantity,
            ReferenceNo = referenceNo,
            MovementDate = DateTime.UtcNow
        });
    }

    public async Task DecreaseAsync(int stockBatchId, int quantity, string referenceNo, CancellationToken ct)
    {
        var batch = await dbContext.StockBatches.FirstOrDefaultAsync(x => x.Id == stockBatchId, ct)
            ?? throw new InvalidOperationException("Batch not found.");

        if (batch.Quantity < quantity)
        {
            throw new InvalidOperationException($"Insufficient stock for batch {batch.BatchNo}.");
        }

        batch.Quantity -= quantity;
        batch.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.StockMovements.Add(new StockMovement
        {
            ProductId = batch.ProductId,
            StockBatchId = batch.Id,
            MovementType = StockMovementType.Sale,
            Quantity = -quantity,
            ReferenceNo = referenceNo,
            MovementDate = DateTime.UtcNow
        });
    }
}


