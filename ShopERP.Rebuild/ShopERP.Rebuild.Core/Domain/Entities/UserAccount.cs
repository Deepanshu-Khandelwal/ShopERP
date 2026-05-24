using ShopERP.Rebuild.Core.Domain.Enums;

namespace ShopERP.Rebuild.Core.Domain.Entities;

public sealed class UserAccount : EntityBase
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;
}
