using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Enums;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(IPaymentService paymentService, ShopErpDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Record([FromBody] PaymentCreateRequest request, CancellationToken ct)
    {
        var payment = await paymentService.RecordPaymentAsync(request, ct);
        return Ok(payment);
    }

    [HttpGet("due/customers")]
    public async Task<IActionResult> CustomerDues(CancellationToken ct)
    {
        var result = await dbContext.CustomerLedgerEntries
            .AsNoTracking()
            .GroupBy(x => new { x.CustomerId, x.Customer.Name })
            .Select(g => new
            {
                g.Key.CustomerId,
                CustomerName = g.Key.Name,
                Balance = g.OrderByDescending(x => x.Id).Select(x => x.Balance).FirstOrDefault()
            })
            .Where(x => x.Balance > 0)
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("due/suppliers")]
    public async Task<IActionResult> SupplierDues(CancellationToken ct)
    {
        var result = await dbContext.SupplierLedgerEntries
            .AsNoTracking()
            .GroupBy(x => new { x.SupplierId, x.Supplier.Name })
            .Select(g => new
            {
                g.Key.SupplierId,
                SupplierName = g.Key.Name,
                Balance = g.OrderByDescending(x => x.Id).Select(x => x.Balance).FirstOrDefault()
            })
            .Where(x => x.Balance > 0)
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] PaymentPartyType? partyType, [FromQuery] int? partyId, CancellationToken ct)
    {
        var query = dbContext.PaymentEntries.AsNoTracking().AsQueryable();
        if (partyType.HasValue)
        {
            query = query.Where(x => x.PartyType == partyType.Value);
        }

        if (partyId.HasValue)
        {
            query = query.Where(x => x.PartyId == partyId.Value);
        }

        return Ok(await query.OrderByDescending(x => x.PaymentDate).ToListAsync(ct));
    }
}


