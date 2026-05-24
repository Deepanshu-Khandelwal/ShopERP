using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class Supplier : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DrugLicenseNo { get; set; } = string.Empty;
    public string GstNo { get; set; } = string.Empty;
}


