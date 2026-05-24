using ShopERP.Rebuild.Core.Domain.Entities;

namespace ShopERP.Rebuild.Core.Contracts;

public interface IAuthenticationService
{
    Task<UserAccount?> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> RegisterAsync(string username, string password, string confirmPassword, CancellationToken cancellationToken = default);
    Task SeedDefaultAdminAsync(CancellationToken cancellationToken = default);
}
