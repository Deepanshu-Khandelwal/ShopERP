using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShopERP.Rebuild.Desktop.Models;
using ShopERP.Rebuild.Desktop.Services;
using System.Collections.ObjectModel;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public partial class ProductsPageViewModel(ProductMasterService productService) : PageViewModelBase("Products")
{
    [ObservableProperty]
    private ObservableCollection<ProductMasterRow> _items = new();

    [ObservableProperty]
    private ObservableCollection<ProductMasterRow> _filteredItems = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusFilter = "All";

    [ObservableProperty]
    private ProductMasterRow? _selectedItem;

    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string _genericName = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _manufacturer = string.Empty;

    [ObservableProperty]
    private string _batchNo = string.Empty;

    [ObservableProperty]
    private DateTime _expiry = DateTime.Today.AddYears(1);

    [ObservableProperty]
    private decimal _gstPercent;

    [ObservableProperty]
    private decimal _mrp;

    [ObservableProperty]
    private decimal _purchaseRate;

    [ObservableProperty]
    private decimal _saleRate;

    [ObservableProperty]
    private int _minStockLevel = 10;

    [ObservableProperty]
    private string _productStatus = "Active";

    [ObservableProperty]
    private string _status = "Ready";

    public IReadOnlyList<string> StatusOptions { get; } = ["All", "Active", "Inactive"];
    public IReadOnlyList<string> ProductStatusOptions { get; } = ["Active", "Inactive"];
    public int RecordCount => FilteredItems.Count;

    public async Task LoadAsync()
    {
        var rows = await productService.ListAsync();
        Items = new ObservableCollection<ProductMasterRow>(rows);
        ApplyFilter();
        Status = $"Loaded {Items.Count} products";
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnStatusFilterChanged(string value) => ApplyFilter();

    partial void OnSelectedItemChanged(ProductMasterRow? value)
    {
        if (value is null)
        {
            return;
        }

        ProductId = value.ProductId;
        ProductName = value.ProductName;
        GenericName = value.GenericName;
        Category = value.Category;
        Manufacturer = value.Manufacturer;
        BatchNo = value.BatchNo;
        Expiry = value.Expiry ?? DateTime.Today.AddYears(1);
        GstPercent = value.GstPercent;
        Mrp = value.Mrp;
        PurchaseRate = value.PurchaseRate;
        SaleRate = value.SaleRate;
        MinStockLevel = value.MinStockLevel;
        ProductStatus = string.IsNullOrWhiteSpace(value.Status) ? "Active" : value.Status;
    }

    [RelayCommand]
    private void NewForm()
    {
        SelectedItem = null;
        ProductId = 0;
        ProductName = string.Empty;
        GenericName = string.Empty;
        Category = string.Empty;
        Manufacturer = string.Empty;
        BatchNo = string.Empty;
        Expiry = DateTime.Today.AddYears(1);
        GstPercent = 0;
        Mrp = 0;
        PurchaseRate = 0;
        SaleRate = 0;
        MinStockLevel = 10;
        ProductStatus = "Active";
        Status = "New product";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ProductName) || string.IsNullOrWhiteSpace(BatchNo))
        {
            Status = "Product Name and Batch No are required.";
            return;
        }

        if (GstPercent < 0 || GstPercent > 28)
        {
            Status = "GST% must be between 0 and 28.";
            return;
        }

        if (Expiry.Date < DateTime.Today)
        {
            Status = "Expiry date cannot be in the past.";
            return;
        }

        if (Mrp <= 0 || PurchaseRate < 0 || SaleRate <= 0)
        {
            Status = "MRP and Sale Rate must be greater than zero.";
            return;
        }

        if (MinStockLevel < 0)
        {
            Status = "Min stock level cannot be negative.";
            return;
        }

        try
        {
            await productService.SaveAsync(
                ProductId > 0 ? ProductId : null,
                ProductName,
                GenericName,
                Category,
                Manufacturer,
                BatchNo,
                Expiry,
                GstPercent,
                Mrp,
                PurchaseRate,
                SaleRate,
                MinStockLevel,
                ProductStatus);
        }
        catch (InvalidOperationException ex)
        {
            Status = ex.Message;
            return;
        }

        await LoadAsync();
        Status = "Product saved";
    }

    [RelayCommand]
    private void Edit(ProductMasterRow? item)
    {
        if (item is null) return;
        SelectedItem = item;
    }

    [RelayCommand]
    private async Task DeleteAsync(ProductMasterRow? item)
    {
        if (item is null) return;
        await productService.DeleteAsync(item.ProductId);
        await LoadAsync();
        Status = "Product removed/inactivated";
    }

    private void ApplyFilter()
    {
        IEnumerable<ProductMasterRow> query = Items;

        if (!string.Equals(StatusFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => string.Equals(x.Status, StatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        var keyword = SearchText.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.BatchNo.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.ProductId.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        FilteredItems = new ObservableCollection<ProductMasterRow>(query.OrderBy(x => x.ProductName));
        OnPropertyChanged(nameof(RecordCount));
    }
}
