using ShopERP.Rebuild.Core.Domain.Enums;

namespace ShopERP.Rebuild.Desktop.Services;

public sealed class UserSession
{
    public string Username { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.Staff;
    public bool IsAuthenticated { get; private set; }

    public void Start(string username, UserRole role)
    {
        Username = username;
        Role = role;
        IsAuthenticated = true;
    }
}
