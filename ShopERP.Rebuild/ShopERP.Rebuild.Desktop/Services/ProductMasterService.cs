using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Rebuild.Desktop.Models;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.Services;

public sealed class ProductMasterService(BackendDbContext dbContext)
{
    public async Task<List<ProductMasterRow>> ListAsync(CancellationToken cancellationToken = default)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var batches = await dbContext.StockBatches
            .AsNoTracking()
            .Where(x => x.Quantity >= 0)
            .ToListAsync(cancellationToken);

        var rows = products
            .Select(product =>
            {
                var productBatches = batches.Where(x => x.ProductId == product.Id).OrderBy(x => x.ExpiryDate).ToList();
                var primaryBatch = productBatches.FirstOrDefault();

                return new ProductMasterRow(
                    product.Id,
                    product.Name,
                    product.GenericName,
                    product.Category,
                    product.Manufacturer,
                    primaryBatch?.BatchNo ?? string.Empty,
                    primaryBatch?.ExpiryDate,
                    productBatches.Sum(x => x.Quantity),
                    product.Mrp,
                    product.Status,
                    product.GstPercent,
                    product.PurchaseRate,
                    product.SaleRate,
                    product.LowStockThreshold);
            })
            .ToList();

        return rows;
    }

    public async Task SaveAsync(
        int? productId,
        string productName,
        string genericName,
        string category,
        string manufacturer,
        string batchNo,
        DateTime expiryDate,
        decimal gstPercent,
        decimal mrp,
        decimal purchaseRate,
        decimal saleRate,
        int minStockLevel,
        string status,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = productName.Trim();
        var normalizedBatch = batchNo.Trim();

        var duplicate = await dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id != (productId ?? 0) && x.Name.ToLower() == normalizedName.ToLower(), cancellationToken);
        if (duplicate)
        {
            throw new InvalidOperationException("Duplicate product name is not allowed.");
        }

        Product model;
        if (productId.HasValue && productId.Value > 0)
        {
            model = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Product not found.");
            model.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            model = new Product
            {
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };
            dbContext.Products.Add(model);
        }

        model.Name = normalizedName;
        model.GenericName = genericName?.Trim() ?? string.Empty;
        model.Category = category?.Trim() ?? string.Empty;
        model.Manufacturer = manufacturer?.Trim() ?? string.Empty;
        model.GstPercent = gstPercent;
        model.Mrp = mrp;
        model.PurchaseRate = purchaseRate;
        model.SaleRate = saleRate;
        model.LowStockThreshold = minStockLevel;
        model.Status = string.IsNullOrWhiteSpace(status) ? "Active" : status.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(normalizedBatch))
        {
            return;
        }

        var existingBatch = await dbContext.StockBatches
            .FirstOrDefaultAsync(x => x.ProductId == model.Id && x.BatchNo.ToLower() == normalizedBatch.ToLower(), cancellationToken);

        if (existingBatch is null)
        {
            var conflictBatch = await dbContext.StockBatches
                .AsNoTracking()
                .AnyAsync(x => x.ProductId != model.Id && x.BatchNo.ToLower() == normalizedBatch.ToLower(), cancellationToken);
            if (conflictBatch)
            {
                throw new InvalidOperationException("Batch number already exists for another product.");
            }

            dbContext.StockBatches.Add(new StockBatch
            {
                ProductId = model.Id,
                BatchNo = normalizedBatch,
                ExpiryDate = expiryDate,
                Quantity = 0,
                Mrp = mrp,
                PurchaseRate = purchaseRate,
                SaleRate = saleRate
            });
        }
        else
        {
            existingBatch.ExpiryDate = expiryDate;
            existingBatch.Mrp = mrp;
            existingBatch.PurchaseRate = purchaseRate;
            existingBatch.SaleRate = saleRate;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);
        if (product is null)
        {
            return;
        }

        var hasSales = await dbContext.SalesItems.AnyAsync(x => x.ProductId == productId, cancellationToken);
        var hasPurchases = await dbContext.PurchaseItems.AnyAsync(x => x.ProductId == productId, cancellationToken);
        if (hasSales || hasPurchases)
        {
            product.Status = "Inactive";
            product.UpdatedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var batches = await dbContext.StockBatches.Where(x => x.ProductId == productId).ToListAsync(cancellationToken);
        dbContext.StockBatches.RemoveRange(batches);
        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}