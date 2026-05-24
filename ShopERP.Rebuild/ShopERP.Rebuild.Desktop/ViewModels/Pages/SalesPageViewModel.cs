using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShopERP.Rebuild.Core.Domain.Entities;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Rebuild.Desktop.Models;
using ShopERP.Rebuild.Desktop.Services;
using ShopERP.Backend.Data;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public partial class SalesPageViewModel(
    SalesCrudService salesService,
    ProductCrudService productService,
    ShopErpDbContext backendDbContext) : PageViewModelBase("Sales")
{
    [ObservableProperty]
    private ObservableCollection<Sale> _items = new();

    [ObservableProperty]
    private ObservableCollection<Sale> _filteredItems = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Sale? _selectedItem;

    [ObservableProperty]
    private string _invoiceNo = string.Empty;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private ObservableCollection<ShopERP.Rebuild.Core.Domain.Entities.Product> _availableProducts = new();

    [ObservableProperty]
    private ObservableCollection<ShopERP.Backend.Domain.Entities.StockBatch> _availableBatches = new();

    [ObservableProperty]
    private ObservableCollection<SalesLineEditorRow> _lines = new();

    [ObservableProperty]
    private SalesLineEditorRow? _selectedLine;

    [ObservableProperty]
    private Guid _selectedProductId;

    [ObservableProperty]
    private int _selectedBatchId;

    [ObservableProperty]
    private int _lineQuantity = 1;

    [ObservableProperty]
    private decimal _lineUnitPrice;

    [ObservableProperty]
    private decimal _lineTaxAmount;

    [ObservableProperty]
    private decimal _lineDiscountAmount;

    [ObservableProperty]
    private string _status = "Ready";

    public int RecordCount => FilteredItems.Count;

    public decimal CalculatedTotal => Lines.Sum(x => x.LineTotal);

    partial void OnLinesChanged(ObservableCollection<SalesLineEditorRow> value)
    {
        value.CollectionChanged += OnLinesCollectionChanged;
        RecalculateTotal();
    }

    partial void OnSelectedProductIdChanged(Guid value)
    {
        // Clear batches and batch selection when product changes
        AvailableBatches.Clear();
        SelectedBatchId = 0;
        LineUnitPrice = 0;
        LineQuantity = 1;

        if (value == Guid.Empty)
            return;

        // Load batches for the selected product
        var product = AvailableProducts.FirstOrDefault(x => x.Id == value);
        if (product is null)
            return;

        // Load batches by product name since IDs don't match between databases
        _ = LoadBatchesAsync(product.Name);
    }

    partial void OnSelectedBatchIdChanged(int value)
    {
        // Populate unit price and quantity from selected batch
        if (value <= 0)
        {
            LineUnitPrice = 0;
            LineQuantity = 1;
            return;
        }

        var batch = AvailableBatches.FirstOrDefault(x => x.Id == value);
        if (batch is not null)
        {
            LineUnitPrice = batch.SaleRate;
            LineQuantity = Math.Min(1, batch.Quantity); // Default to 1, but respect available quantity
        }
    }

    private async Task LoadBatchesAsync(string productName)
    {
        try
        {
            // Get batches for the selected product from the backend database, matched by name
            var batches = await backendDbContext.StockBatches
                .Include(x => x.Product)
                .Where(x => x.Product.Name == productName && x.Quantity > 0)
                .OrderBy(x => x.ExpiryDate)
                .ToListAsync();

            AvailableBatches = new ObservableCollection<ShopERP.Backend.Domain.Entities.StockBatch>(batches);
            
            if (batches.Count == 0)
            {
                Status = "No available batches for this product";
            }
        }
        catch (Exception ex)
        {
            Status = $"Error loading batches: {ex.Message}";
        }
    }

    public async Task LoadAsync()
    {
        var rows = await salesService.ListAsync();
        Items = new ObservableCollection<Sale>(rows);
        ApplyFilter();

        if (AvailableProducts.Count == 0)
        {
            var products = await productService.ListActiveAsync();
            AvailableProducts = new ObservableCollection<ShopERP.Rebuild.Core.Domain.Entities.Product>(products);
        }

        Status = $"Loaded {Items.Count} sales";
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedItemChanged(Sale? value)
    {
        if (value is null)
        {
            return;
        }

        InvoiceNo = value.InvoiceNo;
        Total = value.Total;

        var mapped = value.Lines
            .Select(x => new SalesLineEditorRow
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                TaxAmount = x.TaxAmount,
                DiscountAmount = x.DiscountAmount
            })
            .ToList();

        Lines = new ObservableCollection<SalesLineEditorRow>(mapped);
        foreach (var line in Lines)
        {
            line.PropertyChanged += OnLinePropertyChanged;
        }
        RecalculateTotal();
    }

    [RelayCommand]
    private void NewForm()
    {
        SelectedItem = null;
        InvoiceNo = string.Empty;
        Total = 0;
        Lines = new ObservableCollection<SalesLineEditorRow>();
        SelectedProductId = Guid.Empty;
        SelectedBatchId = 0;
        LineQuantity = 1;
        LineUnitPrice = 0;
        LineTaxAmount = 0;
        LineDiscountAmount = 0;
        AvailableBatches.Clear();
        Status = "New sale";
    }

    [RelayCommand]
    private void AddLine()
    {
        var product = AvailableProducts.FirstOrDefault(x => x.Id == SelectedProductId);
        if (product is null)
        {
            Status = "Select a product before adding line.";
            return;
        }

        var row = new SalesLineEditorRow
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = Math.Max(LineQuantity, 1),
            UnitPrice = LineUnitPrice > 0 ? LineUnitPrice : product.Price,
            TaxAmount = Math.Max(LineTaxAmount, 0),
            DiscountAmount = Math.Max(LineDiscountAmount, 0)
        };
        row.PropertyChanged += OnLinePropertyChanged;

        Lines.Add(row);
        RecalculateTotal();
        Status = "Line added";
    }

    [RelayCommand]
    private void RemoveSelectedLine()
    {
        if (SelectedLine is null)
        {
            return;
        }

        SelectedLine.PropertyChanged -= OnLinePropertyChanged;
        Lines.Remove(SelectedLine);
        RecalculateTotal();
    }

    [RelayCommand]
    private void Edit(Sale? item)
    {
        if (item is null) return;
        SelectedItem = item;
    }

    [RelayCommand]
    private async Task DeleteAsync(Sale? item)
    {
        if (item is null) return;
        await salesService.DeleteAsync(item);
        await LoadAsync();
        Status = "Sale deleted";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(InvoiceNo))
        {
            Status = "Invoice number is required.";
            return;
        }

        if (Lines.Count == 0)
        {
            Status = "Add at least one line item.";
            return;
        }

        var model = SelectedItem ?? new Sale();
        model.InvoiceNo = InvoiceNo.Trim().ToUpperInvariant();
        model.Total = CalculatedTotal;

        var lineModels = Lines.Select(x => new SaleLine
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            TaxAmount = x.TaxAmount,
            DiscountAmount = x.DiscountAmount
        });

        await salesService.SaveAsync(model, lineModels);
        await LoadAsync();
        Status = "Sale saved";
    }

    private void OnLinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<SalesLineEditorRow>())
            {
                item.PropertyChanged += OnLinePropertyChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<SalesLineEditorRow>())
            {
                item.PropertyChanged -= OnLinePropertyChanged;
            }
        }

        RecalculateTotal();
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SalesLineEditorRow.Quantity)
            or nameof(SalesLineEditorRow.UnitPrice)
            or nameof(SalesLineEditorRow.LineTotal))
        {
            RecalculateTotal();
        }
    }

    private void RecalculateTotal()
    {
        Total = CalculatedTotal;
        OnPropertyChanged(nameof(CalculatedTotal));
    }

    private void ApplyFilter()
    {
        var keyword = SearchText.Trim();
        IEnumerable<Sale> query = Items;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.InvoiceNo.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        FilteredItems = new ObservableCollection<Sale>(query.OrderByDescending(x => x.CreatedUtc));
        OnPropertyChanged(nameof(RecordCount));
    }
}
