using CommunityToolkit.Mvvm.ComponentModel;

namespace ShopERP.Rebuild.Desktop.Models;

public sealed partial class PurchaseLineEditorRow : ObservableObject
{
    [ObservableProperty]
    private string _itemName = string.Empty;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private string _batchNo = string.Empty;

    [ObservableProperty]
    private DateTime? _expiryDate;

    [ObservableProperty]
    private decimal _mrp;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _discountAmount;

    public decimal LineTotal => (Quantity * UnitPrice) + TaxAmount - DiscountAmount;

    partial void OnQuantityChanged(int value) => OnPropertyChanged(nameof(LineTotal));
    partial void OnUnitPriceChanged(decimal value) => OnPropertyChanged(nameof(LineTotal));
    partial void OnTaxAmountChanged(decimal value) => OnPropertyChanged(nameof(LineTotal));
    partial void OnDiscountAmountChanged(decimal value) => OnPropertyChanged(nameof(LineTotal));
}
