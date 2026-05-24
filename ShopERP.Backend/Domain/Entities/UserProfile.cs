using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class UserProfile : EntityBase
{
    public int UserId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? DepartmentOrdesignation { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PreferredLanguage { get; set; } = "en";

    // Relationships
    public User User { get; set; } = null!;
}
