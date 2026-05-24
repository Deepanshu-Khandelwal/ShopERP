namespace ShopERP.Backend.Contracts.Requests;

public class PurchaseOrderCreateRequest
{
    public string OrderNo { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public DateTime OrderDate { get; set; }
    public List<PurchaseOrderLineDto> Items { get; set; } = new();
}

public class PurchaseOrderLineDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Rate { get; set; }
}


