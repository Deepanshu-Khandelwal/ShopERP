using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShopERP.Backend.Data;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Infrastructure.Pdf;

public class InvoiceService(ShopErpDbContext dbContext, IConfiguration configuration) : IInvoiceService
{
    public async Task<byte[]> CreateSalesInvoicePdfAsync(int salesBillId, CancellationToken ct)
    {
        var bill = await dbContext.SalesBills
            .Include(x => x.Customer)
            .Include(x => x.Doctor)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == salesBillId, ct)
            ?? throw new InvalidOperationException("Sales bill not found.");

        QuestPDF.Settings.License = LicenseType.Community;
        var shopName = configuration["Shop:Name"] ?? "Medical Store";
        var shopAddress = configuration["Shop:Address"] ?? "Shop Address";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(24);
                page.Size(PageSizes.A4);

                page.Header().Column(col =>
                {
                    col.Item().Text(shopName).FontSize(18).SemiBold();
                    col.Item().Text(shopAddress).FontSize(10);
                    col.Item().Text($"Invoice: {bill.BillNo} | Date: {bill.BillDate:dd-MM-yyyy}").FontSize(10);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Product").SemiBold();
                        header.Cell().Text("Batch").SemiBold();
                        header.Cell().Text("Qty").SemiBold();
                        header.Cell().Text("Rate").SemiBold();
                        header.Cell().Text("Amount").SemiBold();
                    });

                    foreach (var item in bill.Items)
                    {
                        table.Cell().Text(item.Product.Name);
                        table.Cell().Text(item.StockBatchId.ToString());
                        table.Cell().Text(item.Quantity.ToString());
                        table.Cell().Text(item.SaleRate.ToString("0.00"));
                        table.Cell().Text(item.Amount.ToString("0.00"));
                    }
                });

                page.Footer().AlignRight().Text($"Grand Total: {bill.GrandTotal:0.00}").SemiBold();
            });
        }).GeneratePdf();
    }
}


