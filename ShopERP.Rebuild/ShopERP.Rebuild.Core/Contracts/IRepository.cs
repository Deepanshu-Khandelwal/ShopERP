namespace ShopERP.Rebuild.Core.Contracts;

public interface IRepository<T> where T : class
{
    Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
}
