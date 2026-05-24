using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Dtos;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Services;

public class PurchaseOrderService(ShopErpDbContext dbContext, IPurchaseService purchaseService) : IPurchaseOrderService
{
    public async Task<PurchaseOrder> CreateAsync(PurchaseOrderCreateRequest request, CancellationToken ct)
    {
        var order = new PurchaseOrder
        {
            OrderNo = request.OrderNo,
            SupplierId = request.SupplierId,
            OrderDate = request.OrderDate,
            Status = PurchaseOrderStatus.Draft
        };

        foreach (var line in request.Items)
        {
            order.Items.Add(new PurchaseOrderItem
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                Rate = line.Rate,
                Amount = Math.Round(line.Quantity * line.Rate, 2)
            });
        }

        order.TotalAmount = order.Items.Sum(x => x.Amount);
        dbContext.PurchaseOrders.Add(order);
        await dbContext.SaveChangesAsync(ct);
        return order;
    }

    public async Task<PurchaseOrder> MarkSentAsync(int orderId, CancellationToken ct)
    {
        var order = await dbContext.PurchaseOrders.FirstOrDefaultAsync(x => x.Id == orderId, ct)
            ?? throw new InvalidOperationException("Purchase order not found.");

        order.Status = PurchaseOrderStatus.Sent;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return order;
    }

    public async Task<PurchaseBill> ConvertToPurchaseBillAsync(int orderId, string billNo, DateTime billDate, CancellationToken ct)
    {
        var order = await dbContext.PurchaseOrders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == orderId, ct)
            ?? throw new InvalidOperationException("Purchase order not found.");

        var purchaseRequest = new PurchaseBillCreateRequest
        {
            BillNo = billNo,
            SupplierId = order.SupplierId,
            BillDate = billDate,
            LessDiscount = 0,
            RoundOff = 0,
            Items = order.Items.Select(x => new BillLineDto
            {
                ProductId = x.ProductId,
                BatchNo = $"AUTO-{DateTime.UtcNow:yyyyMMdd}-{x.ProductId}",
                Quantity = x.Quantity,
                Rate = x.Rate,
                Mrp = x.Rate,
                GstPercent = 12,
                DiscountPercent = 0,
                ExpiryDate = DateTime.UtcNow.AddYears(1)
            }).ToList()
        };

        var bill = await purchaseService.CreateAsync(purchaseRequest, ct);
        order.Status = PurchaseOrderStatus.Converted;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return bill;
    }
}


