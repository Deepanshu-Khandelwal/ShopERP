namespace ShopERP.Backend.Contracts.Dtos;

public class BillLineDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BatchNo { get; set; } = string.Empty;
    public int? StockBatchId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal Mrp { get; set; }
    public decimal Rate { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal GstPercent { get; set; }
    public decimal Amount { get; set; }
}


