using ShopERP.Backend.Domain.Common;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Domain.Entities;

public class User : EntityBase
{
    public int ShopId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginUtc { get; set; }

    // Relationships
    public Shop Shop { get; set; } = null!;
    public UserProfile? Profile { get; set; }
}
