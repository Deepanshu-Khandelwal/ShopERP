using Microsoft.EntityFrameworkCore;
using ShopERP.Rebuild.Core.Contracts;
using ShopERP.Rebuild.Core.Domain.Entities;
using ShopERP.Rebuild.Core.Domain.Enums;
using ShopERP.Rebuild.Infrastructure.Data;

namespace ShopERP.Rebuild.Infrastructure.Auth;

public sealed class AuthenticationService(ShopErpDbContext dbContext) : IAuthenticationService
{
    public async Task<UserAccount?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim().ToLowerInvariant();
        var user = await dbContext.UserAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Username == normalized && x.IsActive, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var ok = PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        return ok ? user : null;
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(string username, string password, string confirmPassword, CancellationToken cancellationToken = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(username))
            return (false, "Username is required.");

        if (username.Length < 3)
            return (false, "Username must be at least 3 characters.");

        if (username.Length > 50)
            return (false, "Username must not exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required.");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters.");

        if (password != confirmPassword)
            return (false, "Passwords do not match.");

        if (!HasPasswordComplexity(password))
            return (false, "Password must contain uppercase, lowercase, number, and special character.");

        // Check if username exists
        var normalized = username.Trim().ToLowerInvariant();
        var exists = await dbContext.UserAccounts
            .AnyAsync(x => x.Username == normalized, cancellationToken);

        if (exists)
            return (false, "Username already exists.");

        // Hash password and create user
        var (hash, salt) = PasswordHasher.HashPassword(password);
        var user = new UserAccount
        {
            Username = normalized,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = UserRole.Staff,
            IsActive = true
        };

        dbContext.UserAccounts.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task SeedDefaultAdminAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.UserAccounts.AnyAsync(cancellationToken))
        {
            return;
        }

        var (hash, salt) = PasswordHasher.HashPassword("admin@123");
        dbContext.UserAccounts.Add(new UserAccount
        {
            Username = "admin",
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = UserRole.Admin,
            IsActive = true
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool HasPasswordComplexity(string password)
    {
        var hasUppercase = password.Any(char.IsUpper);
        var hasLowercase = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));
        return hasUppercase && hasLowercase && hasDigit && hasSpecial;
    }
}
