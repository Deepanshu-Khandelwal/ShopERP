using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Data;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public class PurchaseOrdersController(IPurchaseOrderService purchaseOrderService, ShopErpDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseOrderCreateRequest request, CancellationToken ct)
    {
        return Ok(await purchaseOrderService.CreateAsync(request, ct));
    }

    [HttpPost("{orderId:int}/send")]
    public async Task<IActionResult> MarkSent(int orderId, CancellationToken ct)
    {
        return Ok(await purchaseOrderService.MarkSentAsync(orderId, ct));
    }

    [HttpPost("{orderId:int}/convert")]
    public async Task<IActionResult> Convert(int orderId, [FromQuery] string billNo, [FromQuery] DateTime billDate, CancellationToken ct)
    {
        return Ok(await purchaseOrderService.ConvertToPurchaseBillAsync(orderId, billNo, billDate, ct));
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var data = await dbContext.PurchaseOrders
            .Include(x => x.Supplier)
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync(ct);
        return Ok(data);
    }
}


