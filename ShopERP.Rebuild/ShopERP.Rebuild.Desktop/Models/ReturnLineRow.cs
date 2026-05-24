using CommunityToolkit.Mvvm.ComponentModel;

namespace ShopERP.Rebuild.Desktop.Models;

public sealed partial class ReturnLineRow : ObservableObject
{
    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private int _stockBatchId;

    [ObservableProperty]
    private string _batchNo = string.Empty;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private decimal _rate;

    public decimal Amount => Math.Round(Quantity * Rate, 2);

    partial void OnQuantityChanged(int value) => OnPropertyChanged(nameof(Amount));
    partial void OnRateChanged(decimal value) => OnPropertyChanged(nameof(Amount));
}
