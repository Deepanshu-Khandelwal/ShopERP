using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Responses;
using ShopERP.Backend.Data;

namespace ShopERP.Backend.Services;

public class ReportService(ShopErpDbContext dbContext) : IReportService
{
    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DateTime fromDate, DateTime toDate, CancellationToken ct)
    {
        var salesQuery = dbContext.SalesBills.Where(x => x.BillDate >= fromDate && x.BillDate <= toDate);
        var purchaseQuery = dbContext.PurchaseBills.Where(x => x.BillDate >= fromDate && x.BillDate <= toDate);

        var totalSales = await salesQuery.SumAsync(x => (decimal?)x.GrandTotal, ct) ?? 0;
        var totalPurchases = await purchaseQuery.SumAsync(x => (decimal?)x.GrandTotal, ct) ?? 0;
        var totalProfit = await salesQuery.SumAsync(x => (decimal?)x.ProfitAmount, ct) ?? 0;
        var totalGst = (await purchaseQuery.SumAsync(x => (decimal?)x.CgstAmount + x.SgstAmount, ct) ?? 0);

        var topProducts = await dbContext.SalesItems
            .Where(x => x.SalesBill.BillDate >= fromDate && x.SalesBill.BillDate <= toDate)
            .GroupBy(x => new { x.ProductId, x.Product.Name })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(10)
            .ToListAsync(ct);

        return new DashboardSummaryResponse
        {
            TotalSales = totalSales,
            TotalPurchases = totalPurchases,
            TotalProfit = totalProfit,
            TotalGst = totalGst,
            TopProducts = topProducts
        };
    }
}


