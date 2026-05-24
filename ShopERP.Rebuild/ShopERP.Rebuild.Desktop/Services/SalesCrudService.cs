using Microsoft.EntityFrameworkCore;
using ShopERP.Rebuild.Core.Domain.Entities;
using ShopERP.Rebuild.Infrastructure.Data;

namespace ShopERP.Rebuild.Desktop.Services;

public sealed class SalesCrudService(ShopErpDbContext dbContext)
{
    public Task<List<Sale>> ListAsync(CancellationToken cancellationToken = default)
        => dbContext.Sales
            .Include(x => x.Lines)
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);

    public async Task SaveAsync(Sale model, IEnumerable<SaleLine> lines, CancellationToken cancellationToken = default)
    {
        var lineList = lines.ToList();
        model.Total = lineList.Sum(x => x.LineTotal);
        model.UpdatedUtc = DateTime.UtcNow;

        var existing = await dbContext.Sales.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken);
        if (existing is null)
        {
            model.CreatedUtc = DateTime.UtcNow;
            model.Lines = lineList;
            dbContext.Sales.Add(model);
        }
        else
        {
            existing.InvoiceNo = model.InvoiceNo;
            existing.Total = model.Total;
            existing.UpdatedUtc = model.UpdatedUtc;

            dbContext.SaleLines.RemoveRange(existing.Lines);
            existing.Lines = lineList;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Sale model, CancellationToken cancellationToken = default)
    {
        dbContext.Sales.Remove(model);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
