using Microsoft.EntityFrameworkCore;
using ShopERP.Rebuild.Core.Domain.Entities;
using ShopERP.Rebuild.Infrastructure.Data;

namespace ShopERP.Rebuild.Desktop.Services;

public sealed class CustomerCrudService(ShopErpDbContext dbContext)
{
    public Task<List<Customer>> ListAsync(CancellationToken cancellationToken = default)
        => dbContext.Customers.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task SaveAsync(Customer model, CancellationToken cancellationToken = default)
    {
        model.UpdatedUtc = DateTime.UtcNow;

        var exists = await dbContext.Customers.AnyAsync(x => x.Id == model.Id, cancellationToken);
        if (!exists)
        {
            model.CreatedUtc = DateTime.UtcNow;
            dbContext.Customers.Add(model);
        }
        else
        {
            dbContext.Customers.Update(model);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Customer model, CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Remove(model);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
