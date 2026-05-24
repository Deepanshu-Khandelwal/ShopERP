using ShopERP.Backend.Contracts.Dtos;

namespace ShopERP.Backend.Contracts.Requests;

public class PurchaseBillCreateRequest
{
    public string BillNo { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public DateTime BillDate { get; set; }
    public decimal LessDiscount { get; set; }
    public decimal RoundOff { get; set; }
    public List<BillLineDto> Items { get; set; } = new();
}


