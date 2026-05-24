using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class Product : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g. Tablet, Syrup, Injectable
    public string Sku { get; set; } = string.Empty;
    public string HsnCode { get; set; } = string.Empty;
    
    public decimal GstPercent { get; set; }
    public decimal Mrp { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal SaleRate { get; set; }
    
    public int LowStockThreshold { get; set; } = 10;
    public bool IsPrescriptionRequired { get; set; } = false;
    public string Status { get; set; } = "Active";
}


