using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class Shop : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GstNo { get; set; } = string.Empty;
    public string RegistrationNo { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Relationships
    public ICollection<User> Users { get; set; } = new List<User>();
}
