using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class Customer : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty; // Encrypted in DB
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string MedicalHistory { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
}


