using ShopERP.Backend.Domain.Entities;

namespace ShopERP.Rebuild.Desktop.Services;

public sealed record PurchaseOrderLineDraft(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Rate);

public sealed record PurchaseOrderDraft(
    int PurchaseOrderId,
    int SupplierId,
    string OrderNo,
    DateTime OrderDate,
    List<PurchaseOrderLineDraft> Lines);

public sealed class PurchaseOrderDraftStore
{
    public PurchaseOrderDraft? Current { get; private set; }

    public void SetFrom(PurchaseOrder purchaseOrder)
    {
        Current = new PurchaseOrderDraft(
            purchaseOrder.Id,
            purchaseOrder.SupplierId,
            purchaseOrder.OrderNo,
            purchaseOrder.OrderDate,
            purchaseOrder.Items
                .Select(item => new PurchaseOrderLineDraft(
                    item.ProductId,
                    item.Product?.Name ?? string.Empty,
                    item.Quantity,
                    item.Rate))
                .ToList());
    }

    public void Clear()
    {
        Current = null;
    }
}