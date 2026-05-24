using Microsoft.EntityFrameworkCore;
using ShopERP.Rebuild.Core.Domain.Entities;
using ShopERP.Rebuild.Infrastructure.Data;

namespace ShopERP.Rebuild.Desktop.Services;

public sealed class ProductCrudService(ShopErpDbContext dbContext)
{
    public Task<List<Product>> ListActiveAsync(CancellationToken cancellationToken = default)
        => dbContext.Products
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<List<Product>> ListAsync(CancellationToken cancellationToken = default)
        => dbContext.Products.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task SaveAsync(Product model, CancellationToken cancellationToken = default)
    {
        model.UpdatedUtc = DateTime.UtcNow;

        var exists = await dbContext.Products.AnyAsync(x => x.Id == model.Id, cancellationToken);
        if (!exists)
        {
            model.CreatedUtc = DateTime.UtcNow;
            dbContext.Products.Add(model);
        }
        else
        {
            dbContext.Products.Update(model);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product model, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Remove(model);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
