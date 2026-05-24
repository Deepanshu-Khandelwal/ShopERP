using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Contracts.Responses;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ShopERP.Backend.Services;

public class AuthenticationService(ShopErpDbContext dbContext) : IAuthenticationService
{
    public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken ct)
    {
        var user = await dbContext.Users
            .Include(u => u.Shop)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, cancellationToken: ct);

        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        user.LastLoginUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            User = MapToUserDto(user),
            Token = GenerateJwtToken(user)
        };
    }

    public async Task<UserCreateResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        // Verify shop exists
        var shop = await dbContext.Shops.FindAsync(new object[] { request.ShopId }, cancellationToken: ct);
        if (shop == null)
        {
            return new UserCreateResponse
            {
                Success = false,
                Message = "Shop not found"
            };
        }

        // Check if username or email already exists
        if (await dbContext.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email, cancellationToken: ct))
        {
            return new UserCreateResponse
            {
                Success = false,
                Message = "Username or email already exists"
            };
        }

        var user = new User
        {
            ShopId = request.ShopId,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Role = (UserRole)request.Role,
            IsActive = true,
            Shop = shop
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);

        return new UserCreateResponse
        {
            Success = true,
            Message = "User created successfully",
            User = MapToUserDto(user)
        };
    }

    public async Task<ShopCreateResponse> CreateShopAsync(CreateShopRequest request, CancellationToken ct)
    {
        // Check if shop name already exists
        if (await dbContext.Shops.AnyAsync(s => s.Name == request.Name, cancellationToken: ct))
        {
            return new ShopCreateResponse
            {
                Success = false,
                Message = "Shop name already exists"
            };
        }

        var shop = new Shop
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Phone = request.Phone,
            Email = request.Email,
            GstNo = request.GstNo,
            RegistrationNo = request.RegistrationNo,
            IsActive = true
        };

        dbContext.Shops.Add(shop);
        await dbContext.SaveChangesAsync(ct);

        return new ShopCreateResponse
        {
            Success = true,
            Message = "Shop created successfully",
            Shop = MapToShopDto(shop)
        };
    }

    public async Task<UserDto?> GetUserAsync(int userId, CancellationToken ct)
    {
        var user = await dbContext.Users
            .Include(u => u.Shop)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: ct);

        return user == null ? null : MapToUserDto(user);
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hash;
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            ShopId = user.ShopId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Role = (int)user.Role,
            RoleName = user.Role.ToString(),
            IsActive = user.IsActive,
            LastLoginUtc = user.LastLoginUtc,
            Shop = user.Shop == null ? null : MapToShopDto(user.Shop),
            Profile = user.Profile == null ? null : MapToProfileDto(user.Profile)
        };
    }

    private ShopDto MapToShopDto(Shop shop)
    {
        return new ShopDto
        {
            Id = shop.Id,
            Name = shop.Name,
            Address = shop.Address,
            City = shop.City,
            State = shop.State,
            ZipCode = shop.ZipCode,
            Phone = shop.Phone,
            Email = shop.Email,
            GstNo = shop.GstNo,
            RegistrationNo = shop.RegistrationNo,
            IsActive = shop.IsActive
        };
    }

    private UserProfileDto MapToProfileDto(UserProfile profile)
    {
        return new UserProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            ZipCode = profile.ZipCode,
            ProfilePictureUrl = profile.ProfilePictureUrl,
            DepartmentOrDesignation = profile.DepartmentOrdesignation,
            DateOfBirth = profile.DateOfBirth,
            PreferredLanguage = profile.PreferredLanguage
        };
    }

    private string GenerateJwtToken(User user)
    {
        // Simple token generation - in production, use proper JWT library
        var tokenData = $"{user.Id}:{user.Username}:{DateTime.UtcNow.AddHours(24):O}";
        var tokenBytes = Encoding.UTF8.GetBytes(tokenData);
        return Convert.ToBase64String(tokenBytes);
    }
}
