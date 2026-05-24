using Microsoft.EntityFrameworkCore;
using ShopERP.Rebuild.Core.Contracts;
using ShopERP.Rebuild.Infrastructure.Data;

namespace ShopERP.Rebuild.Infrastructure.Repositories;

public sealed class EfRepository<T>(ShopErpDbContext dbContext) : IRepository<T> where T : class
{
    public Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
        => dbContext.Set<T>().AsNoTracking().ToListAsync(cancellationToken);

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Set<T>().FindAsync([id], cancellationToken).AsTask();

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<T>().Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<T>().Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
