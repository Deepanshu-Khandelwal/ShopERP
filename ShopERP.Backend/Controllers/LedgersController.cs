using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/ledgers")]
public class LedgersController(ShopErpDbContext dbContext) : ControllerBase
{
    [HttpGet("supplier/{supplierId:int}")]
    public async Task<IActionResult> SupplierLedger(int supplierId, CancellationToken ct)
    {
        var entries = await dbContext.SupplierLedgerEntries
            .AsNoTracking()
            .Where(x => x.SupplierId == supplierId)
            .OrderBy(x => x.EntryDate)
            .ToListAsync(ct);

        return Ok(entries);
    }

    [HttpGet("customer/{customerId:int}")]
    public async Task<IActionResult> CustomerLedger(int customerId, CancellationToken ct)
    {
        var entries = await dbContext.CustomerLedgerEntries
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.EntryDate)
            .ToListAsync(ct);

        return Ok(entries);
    }
}


