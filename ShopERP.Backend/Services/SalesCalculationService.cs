using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Contracts.Dtos;

namespace ShopERP.Backend.Services;

public class SalesCalculationService : ISalesCalculationService
{
    public SalesItem BuildSalesItem(StockBatch batch, BillLineDto ri)
    {
        var qty = ri.Quantity;
        var rate = ri.Rate > 0 ? ri.Rate : batch.SaleRate;
        var disc = ri.DiscountPercent;
        var gstPer = ri.GstPercent;

        var totalBeforeDisc = Math.Round(qty * rate, 2);
        var discAmt = Math.Round(totalBeforeDisc * disc / 100m, 2);
        var taxable = totalBeforeDisc - discAmt;
        var totalGst = Math.Round(taxable * gstPer / 100m, 2);

        var lineProfit = Math.Round(qty * (rate - batch.PurchaseRate), 2) - discAmt;

        return new SalesItem
        {
            ProductId = ri.ProductId,
            StockBatchId = batch.Id,
            Quantity = qty,
            Mrp = ri.Mrp > 0 ? ri.Mrp : batch.Mrp,
            SaleRate = rate,
            PurchaseRate = batch.PurchaseRate,
            DiscountPercent = disc,
            GstPercent = gstPer,
            TaxableAmount = taxable,
            GstAmount = totalGst,
            CgstAmount = Math.Round(totalGst / 2m, 2),
            SgstAmount = Math.Round(totalGst / 2m, 2),
            Amount = taxable + totalGst,
            ProfitAmount = lineProfit
        };
    }

    public void ApplyBillTotals(SalesBill bill, decimal requestedPaidAmount)
    {
        bill.Subtotal = Math.Round(bill.Items.Sum(x => x.TaxableAmount), 2);
        bill.DiscountAmount = Math.Round(bill.Subtotal * bill.DiscountPercent / 100m, 2);
        
        var taxableAfterBillDisc = bill.Subtotal - bill.DiscountAmount;
        bill.CgstAmount = Math.Round(bill.Items.Sum(x => x.CgstAmount), 2);
        bill.SgstAmount = Math.Round(bill.Items.Sum(x => x.SgstAmount), 2);
        bill.TotalGstAmount = bill.CgstAmount + bill.SgstAmount;

        bill.GrandTotal = Math.Round(taxableAfterBillDisc + bill.TotalGstAmount, 0, MidpointRounding.AwayFromZero);
        bill.ProfitAmount = Math.Round(bill.Items.Sum(x => x.ProfitAmount) - bill.DiscountAmount, 2);
        bill.PaidAmount = Math.Min(requestedPaidAmount, bill.GrandTotal);
        bill.DueAmount = bill.GrandTotal - bill.PaidAmount;
    }
}


