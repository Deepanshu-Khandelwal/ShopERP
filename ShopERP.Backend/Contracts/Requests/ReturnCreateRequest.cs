namespace ShopERP.Backend.Contracts.Requests;

public class ReturnCreateRequest
{
    public string ReturnNo { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime ReturnDate { get; set; }
    public List<ReturnLineDto> Items { get; set; } = new();
}

public class ReturnLineDto
{
    public int ProductId { get; set; }
    public int StockBatchId { get; set; }
    public int Quantity { get; set; }
    public decimal Rate { get; set; }
}


