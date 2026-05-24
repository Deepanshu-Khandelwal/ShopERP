using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IReportService reportService, ShopErpDbContext dbContext) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken ct)
    {
        return Ok(await reportService.GetDashboardSummaryAsync(fromDate, toDate, ct));
    }

    [HttpGet("gst")]
    public async Task<IActionResult> GstReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken ct)
    {
        var purchase = await dbContext.PurchaseBills
            .AsNoTracking()
            .Where(x => x.BillDate >= fromDate && x.BillDate <= toDate)
            .Select(x => new
            {
                x.BillNo,
                x.BillDate,
                x.CgstAmount,
                x.SgstAmount,
                TotalGst = x.CgstAmount + x.SgstAmount
            })
            .ToListAsync(ct);

        return Ok(purchase);
    }

    [HttpGet("profit")]
    public async Task<IActionResult> Profit([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken ct)
    {
        var bills = await dbContext.SalesBills
            .AsNoTracking()
            .Where(x => x.BillDate >= fromDate && x.BillDate <= toDate)
            .Select(x => new
            {
                x.BillNo,
                x.BillDate,
                x.ProfitAmount,
                x.GrandTotal
            })
            .ToListAsync(ct);

        return Ok(new
        {
            Bills = bills,
            TotalProfit = bills.Sum(x => x.ProfitAmount)
        });
    }

    [HttpGet("expired-stock")]
    public async Task<IActionResult> ExpiredStock(CancellationToken ct)
    {
        var now = DateTime.UtcNow.Date;
        var data = await dbContext.StockBatches.AsNoTracking().Include(x => x.Product)
            .Where(x => x.ExpiryDate < now)
            .ToListAsync(ct);

        return Ok(data);
    }
}


