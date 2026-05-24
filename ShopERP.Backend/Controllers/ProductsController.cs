using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ShopErpDbContext dbContext, IStockService stockService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken ct)
    {
        var query = dbContext.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x => x.Name.Contains(q));
        }

        return Ok(await query.OrderBy(x => x.Name).ToListAsync(ct));
    }

    [HttpPost("direct-entry")]
    public async Task<IActionResult> DirectEntry([FromBody] ProductEntryRequest request, CancellationToken ct)
    {
        Product product;
        if (request.ProductId.HasValue)
        {
            product = await dbContext.Products.FirstAsync(x => x.Id == request.ProductId.Value, ct);
        }
        else
        {
            product = new Product
            {
                Name = request.ProductName,
                Mrp = request.Mrp,
                PurchaseRate = request.Rate,
                SaleRate = request.Rate
            };
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(ct);
        }

        await stockService.IncreaseAsync(product.Id, request.BatchNo, request.ExpiryDate, request.Quantity, request.Rate, request.Rate, request.Mrp, StockMovementType.DirectEntry, request.ReferenceNo, ct);
        await dbContext.SaveChangesAsync(ct);

        return Ok(new { product.Id, product.Name });
    }

    [HttpGet("stock")]
    public async Task<IActionResult> Stock([FromQuery] string? product, [FromQuery] string? batch, CancellationToken ct)
    {
        var query = dbContext.StockBatches.AsNoTracking().Include(x => x.Product).AsQueryable();
        if (!string.IsNullOrWhiteSpace(product))
        {
            query = query.Where(x => x.Product.Name.Contains(product));
        }

        if (!string.IsNullOrWhiteSpace(batch))
        {
            query = query.Where(x => x.BatchNo.Contains(batch));
        }

        return Ok(await query.OrderBy(x => x.Product.Name).ThenBy(x => x.BatchNo).ToListAsync(ct));
    }

    [HttpGet("expiry")]
    public async Task<IActionResult> Expiry([FromQuery] int days = 90, CancellationToken ct = default)
    {
        var toDate = DateTime.UtcNow.Date.AddDays(days);
        var items = await dbContext.StockBatches
            .AsNoTracking()
            .Include(x => x.Product)
            .Where(x => x.ExpiryDate <= toDate)
            .OrderBy(x => x.ExpiryDate)
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> LowStock(CancellationToken ct)
    {
        var items = await dbContext.StockBatches
            .AsNoTracking()
            .Include(x => x.Product)
            .Where(x => x.Quantity <= x.Product.LowStockThreshold)
            .ToListAsync(ct);

        return Ok(items);
    }
}

public class ProductEntryRequest
{
    public int? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Mrp { get; set; }
    public decimal Rate { get; set; }
    public int Quantity { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddYears(1);
    public string ReferenceNo { get; set; } = "DIRECT";
}


