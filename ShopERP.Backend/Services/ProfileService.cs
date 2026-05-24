using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Contracts.Responses;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ShopERP.Backend.Services;

public class ProfileService(ShopErpDbContext dbContext) : IProfileService
{
    public async Task<UserProfileDto?> GetProfileAsync(int userId, CancellationToken ct)
    {
        var profile = await dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken: ct);

        return profile == null ? null : MapToProfileDto(profile);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct)
    {
        var user = await dbContext.Users.FindAsync(new object[] { userId }, cancellationToken: ct);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var profile = await dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken: ct);

        if (profile == null)
        {
            profile = new UserProfile { UserId = userId };
            dbContext.UserProfiles.Add(profile);
        }

        profile.Address = request.Address ?? profile.Address;
        profile.City = request.City ?? profile.City;
        profile.State = request.State ?? profile.State;
        profile.ZipCode = request.ZipCode ?? profile.ZipCode;
        profile.DepartmentOrdesignation = request.DepartmentOrDesignation ?? profile.DepartmentOrdesignation;
        profile.PreferredLanguage = request.PreferredLanguage ?? profile.PreferredLanguage;
        profile.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return MapToProfileDto(profile);
    }

    public async Task<UserDto> GetUserWithProfileAsync(int userId, CancellationToken ct)
    {
        var user = await dbContext.Users
            .Include(u => u.Shop)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: ct);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        return MapToUserDto(user);
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
}
