using Microsoft.EntityFrameworkCore;
using ShopERP.Rebuild.Core.Domain.Entities;
using ShopERP.Rebuild.Infrastructure.Data;

namespace ShopERP.Rebuild.Desktop.Services;

public sealed class PurchasesCrudService(ShopErpDbContext dbContext)
{
    public Task<List<Purchase>> ListAsync(CancellationToken cancellationToken = default)
        => dbContext.Purchases
            .Include(x => x.Lines)
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);

    public async Task SaveAsync(Purchase model, IEnumerable<PurchaseLine> lines, CancellationToken cancellationToken = default)
    {
        var lineList = lines.ToList();
        model.Total = lineList.Sum(x => x.LineTotal);
        model.UpdatedUtc = DateTime.UtcNow;

        var existing = await dbContext.Purchases.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken);
        if (existing is null)
        {
            model.CreatedUtc = DateTime.UtcNow;
            model.Lines = lineList;
            dbContext.Purchases.Add(model);
        }
        else
        {
            existing.BillNo = model.BillNo;
            existing.SupplierName = model.SupplierName;
            existing.Total = model.Total;
            existing.Status = model.Status;
            existing.UpdatedUtc = model.UpdatedUtc;

            dbContext.PurchaseLines.RemoveRange(existing.Lines);
            existing.Lines = lineList;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Purchase model, CancellationToken cancellationToken = default)
    {
        dbContext.Purchases.Remove(model);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
