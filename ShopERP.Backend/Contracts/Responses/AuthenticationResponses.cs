namespace ShopERP.Backend.Contracts.Responses;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public string? Token { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public ShopDto? Shop { get; set; }
    public UserProfileDto? Profile { get; set; }
}

public class ShopDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GstNo { get; set; } = string.Empty;
    public string RegistrationNo { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UserProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? DepartmentOrDesignation { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PreferredLanguage { get; set; }
}

public class ShopCreateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ShopDto? Shop { get; set; }
}

public class UserCreateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
}
