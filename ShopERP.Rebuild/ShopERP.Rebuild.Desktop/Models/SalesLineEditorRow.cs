using CommunityToolkit.Mvvm.ComponentModel;

namespace ShopERP.Rebuild.Desktop.Models;

public sealed partial class SalesLineEditorRow : ObservableObject
{
    [ObservableProperty]
    private Guid _productId;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private decimal _unitPrice;

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
