using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Data;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController(ISalesService salesService, IInvoiceService invoiceService, ShopErpDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SalesBillCreateRequest request, CancellationToken ct)
    {
        var bill = await salesService.CreateAsync(request, ct);
        return Ok(bill);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken ct)
    {
        var query = dbContext.SalesBills.AsNoTracking().Include(x => x.Customer).Include(x => x.Doctor).AsQueryable();
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

    [HttpGet("doctor-wise")]
    public async Task<IActionResult> DoctorWise([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken ct)
    {
        var from = fromDate ?? DateTime.UtcNow.Date.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow.Date;

        var data = await dbContext.SalesBills
            .AsNoTracking()
            .Where(x => x.BillDate >= from && x.BillDate <= to && x.DoctorId != null)
            .GroupBy(x => new { x.DoctorId, x.Doctor!.Name })
            .Select(g => new
            {
                DoctorId = g.Key.DoctorId,
                DoctorName = g.Key.Name,
                TotalSales = g.Sum(x => x.GrandTotal),
                TotalBills = g.Count()
            })
            .OrderByDescending(x => x.TotalSales)
            .ToListAsync(ct);

        return Ok(data);
    }

    [HttpGet("customer-history/{customerId:int}")]
    public async Task<IActionResult> CustomerHistory(int customerId, CancellationToken ct)
    {
        var history = await dbContext.SalesBills
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.BillDate)
            .ToListAsync(ct);

        return Ok(history);
    }

    [HttpGet("repeat-from-bill/{salesBillId:int}")]
    public async Task<IActionResult> RepeatFromBill(int salesBillId, CancellationToken ct)
    {
        var bill = await dbContext.SalesBills
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == salesBillId, ct);

        if (bill is null)
        {
            return NotFound();
        }

        var template = bill.Items.Select(x => new
        {
            x.ProductId,
            x.StockBatchId,
            x.Quantity,
            x.SaleRate
        });

        return Ok(template);
    }

    [HttpGet("invoice/{salesBillId:int}")]
    public async Task<IActionResult> Invoice(int salesBillId, CancellationToken ct)
    {
        var bytes = await invoiceService.CreateSalesInvoicePdfAsync(salesBillId, ct);
        return File(bytes, "application/pdf", $"invoice_{salesBillId}.pdf");
    }
}


