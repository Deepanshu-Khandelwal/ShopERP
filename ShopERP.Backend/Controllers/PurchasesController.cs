using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Data;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/purchases")]
public class PurchasesController(IPurchaseService purchaseService, ShopErpDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseBillCreateRequest request, CancellationToken ct)
    {
        var bill = await purchaseService.CreateAsync(request, ct);
        return Ok(bill);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken ct)
    {
        var query = dbContext.PurchaseBills
            .AsNoTracking()
            .Include(x => x.Supplier)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.BillDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.BillDate <= toDate.Value);
        }

        return Ok(await query.OrderByDescending(x => x.BillDate).ToListAsync(ct));
    }
}


