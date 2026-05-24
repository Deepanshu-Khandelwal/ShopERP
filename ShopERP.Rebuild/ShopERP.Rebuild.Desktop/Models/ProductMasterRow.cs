namespace ShopERP.Rebuild.Desktop.Models;

public sealed record ProductMasterRow(
    int ProductId,
    string ProductName,
    string GenericName,
    string Category,
    string Manufacturer,
    string BatchNo,
    DateTime? Expiry,
    int StockQty,
    decimal Mrp,
    string Status,
    decimal GstPercent,
    decimal PurchaseRate,
    decimal SaleRate,
    int MinStockLevel);