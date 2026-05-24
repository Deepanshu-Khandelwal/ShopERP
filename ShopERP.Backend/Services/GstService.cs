namespace ShopERP.Backend.Services;

public class GstService : IGstService
{
    public (decimal cgst, decimal sgst, decimal totalGst) Calculate(decimal taxableAmount, decimal gstPercent)
    {
        var totalGst = Math.Round(taxableAmount * gstPercent / 100m, 2, MidpointRounding.AwayFromZero);
        var cgst = Math.Round(totalGst / 2m, 2, MidpointRounding.AwayFromZero);
        var sgst = totalGst - cgst;
        return (cgst, sgst, totalGst);
    }
}


