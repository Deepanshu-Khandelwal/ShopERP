using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;
using ShopERP.Backend.Services;
using System.Security.Cryptography;
using System.Text;

namespace ShopERP.Backend.Data;

public static class DbInitializer
{
    public static async Task SeedDefaultDataAsync(ShopErpDbContext dbContext, IAuthenticationService authService, CancellationToken ct)
    {
        await dbContext.Database.EnsureCreatedAsync(ct);

        // Check if data already exists
        if (dbContext.Shops.Any())
        {
            return; // Database already seeded
        }

        // Create default shop
        var defaultShop = new Shop
        {
            Name = "ShopERP Default Store",
            Address = "123 Main Street",
            City = "City",
            State = "State",
            ZipCode = "12345",
            Phone = "1234567890",
            Email = "shop@shoperp.com",
            GstNo = "GST123456789",
            RegistrationNo = "REG123456",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Shops.Add(defaultShop);
        await dbContext.SaveChangesAsync(ct);

        // Create default admin user
        var adminUser = new User
        {
            ShopId = defaultShop.Id,
            Username = "admin",
            Email = "admin@shoperp.com",
            PasswordHash = authService.HashPassword("Admin@123"),
            FullName = "Administrator",
            PhoneNumber = "1234567890",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync(ct);

        // Create admin profile
        var adminProfile = new UserProfile
        {
            UserId = adminUser.Id,
            Address = "123 Admin Street",
            City = "City",
            State = "State",
            ZipCode = "12345",
            DepartmentOrdesignation = "System Administrator",
            PreferredLanguage = "en",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.UserProfiles.Add(adminProfile);
        await dbContext.SaveChangesAsync(ct);
    }
}
