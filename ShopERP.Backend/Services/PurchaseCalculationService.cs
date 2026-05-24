using ShopERP.Backend.Contracts.Dtos;
using ShopERP.Backend.Domain.Entities;

namespace ShopERP.Backend.Services;

public class PurchaseCalculationService(IGstService gstService) : IPurchaseCalculationService
{
    public PurchaseLineComputation BuildPurchaseItem(BillLineDto item)
    {
        var lineAmount = Math.Round(item.Quantity * item.Rate, 2, MidpointRounding.AwayFromZero);
        var lineDiscount = Math.Round(lineAmount * item.DiscountPercent / 100m, 2, MidpointRounding.AwayFromZero);
        var taxable = lineAmount - lineDiscount;
        var gst = gstService.Calculate(taxable, item.GstPercent);

        var purchaseItem = new PurchaseItem
        {
            ProductId = item.ProductId,
            BatchNo = item.BatchNo,
            Quantity = item.Quantity,
            Mrp = item.Mrp,
            Rate = item.Rate,
            ExpiryDate = item.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
            DiscountPercent = item.DiscountPercent,
            GstPercent = item.GstPercent,
            Amount = taxable + gst.totalGst
        };

        return new PurchaseLineComputation(purchaseItem, taxable, gst.cgst, gst.sgst);
    }

    public void ApplyBillTotals(PurchaseBill bill, decimal lessDiscount, decimal roundOff, decimal subtotal, decimal cgst, decimal sgst)
    {
        bill.Subtotal = Math.Round(subtotal, 2);
        bill.DiscountAmount = lessDiscount;
        bill.CgstAmount = Math.Round(cgst, 2);
        bill.SgstAmount = Math.Round(sgst, 2);
        bill.RoundOff = roundOff;
        bill.GrandTotal = Math.Round((bill.Subtotal - bill.DiscountAmount) + bill.CgstAmount + bill.SgstAmount + bill.RoundOff, 2);
    }
}

public sealed record PurchaseLineComputation(PurchaseItem Item, decimal Taxable, decimal Cgst, decimal Sgst);


