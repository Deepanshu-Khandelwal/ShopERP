using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class Doctor : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty; // License / Registration No
    public string Specialty { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty; // e.g. MBBS, MD
    public string Mobile { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ClinicVisitDetails { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}


