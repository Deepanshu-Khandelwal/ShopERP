using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShopERP.Rebuild.Core.Domain.Entities;
using ShopERP.Rebuild.Desktop.Models;
using ShopERP.Rebuild.Desktop.Services;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public partial class PurchasesPageViewModel(PurchasesCrudService purchasesService) : PageViewModelBase("Purchases")
{
    [ObservableProperty]
    private ObservableCollection<Purchase> _items = new();

    [ObservableProperty]
    private Purchase? _selectedItem;

    [ObservableProperty]
    private string _billNo = string.Empty;

    [ObservableProperty]
    private string _supplierName = string.Empty;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private string _purchaseStatus = "Open";

    [ObservableProperty]
    private ObservableCollection<PurchaseLineEditorRow> _lines = new();

    [ObservableProperty]
    private PurchaseLineEditorRow? _selectedLine;

    [ObservableProperty]
    private string _lineItemName = string.Empty;

    [ObservableProperty]
    private int _lineQuantity = 1;

    [ObservableProperty]
    private decimal _lineUnitPrice;

    [ObservableProperty]
    private string _lineBatchNo = string.Empty;

    [ObservableProperty]
    private DateTime _lineExpiryDate = DateTime.Today.AddYears(1);

    [ObservableProperty]
    private decimal _lineMrp;

    [ObservableProperty]
    private decimal _lineTaxAmount;

    [ObservableProperty]
    private decimal _lineDiscountAmount;

    [ObservableProperty]
    private string _status = "Ready";

    public decimal CalculatedTotal => Lines.Sum(x => x.LineTotal);

    partial void OnLinesChanged(ObservableCollection<PurchaseLineEditorRow> value)
    {
        value.CollectionChanged += OnLinesCollectionChanged;
        RecalculateTotal();
    }

    public async Task LoadAsync()
    {
        var rows = await purchasesService.ListAsync();
        Items = new ObservableCollection<Purchase>(rows);
        Status = $"Loaded {Items.Count} purchases";
    }

    partial void OnSelectedItemChanged(Purchase? value)
    {
        if (value is null)
        {
            return;
        }

        BillNo = value.BillNo;
        SupplierName = value.SupplierName;
        Total = value.Total;
        PurchaseStatus = value.Status;

        var mapped = value.Lines
            .Select(x => new PurchaseLineEditorRow
            {
                ItemName = x.ItemName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                BatchNo = x.BatchNo,
                ExpiryDate = x.ExpiryDate,
                Mrp = x.Mrp,
                TaxAmount = x.TaxAmount,
                DiscountAmount = x.DiscountAmount
            })
            .ToList();
        Lines = new ObservableCollection<PurchaseLineEditorRow>(mapped);
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
        BillNo = string.Empty;
        SupplierName = string.Empty;
        Total = 0;
        PurchaseStatus = "Open";
        Lines = new ObservableCollection<PurchaseLineEditorRow>();
        LineItemName = string.Empty;
        LineBatchNo = string.Empty;
        LineExpiryDate = DateTime.Today.AddYears(1);
        LineMrp = 0;
        LineQuantity = 1;
        LineUnitPrice = 0;
        LineTaxAmount = 0;
        LineDiscountAmount = 0;
        Status = "New clinical procurement";
    }

    [RelayCommand]
    private void AddLine()
    {
        if (string.IsNullOrWhiteSpace(LineItemName))
        {
            Status = "Enter item name before adding line.";
            return;
        }

        var row = new PurchaseLineEditorRow
        {
            ItemName = LineItemName.Trim(),
            Quantity = Math.Max(LineQuantity, 1),
            UnitPrice = Math.Max(LineUnitPrice, 0),
            BatchNo = LineBatchNo.Trim(),
            ExpiryDate = LineExpiryDate,
            Mrp = LineMrp,
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
    private void Edit(Purchase? item)
    {
        if (item is null) return;
        SelectedItem = item;
    }

    [RelayCommand]
    private async Task DeleteAsync(Purchase? item)
    {
        if (item is null) return;
        await purchasesService.DeleteAsync(item);
        await LoadAsync();
        Status = "Purchase deleted";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(BillNo) || string.IsNullOrWhiteSpace(SupplierName))
        {
            Status = "Bill No and Supplier are required.";
            return;
        }

        if (Lines.Count == 0)
        {
            Status = "Add at least one line item.";
            return;
        }

        var model = SelectedItem ?? new Purchase();
        model.BillNo = BillNo.Trim().ToUpperInvariant();
        model.SupplierName = SupplierName.Trim();
        model.Total = CalculatedTotal;
        model.Status = string.IsNullOrWhiteSpace(PurchaseStatus) ? "Open" : PurchaseStatus.Trim();

        var lineModels = Lines.Select(x => new PurchaseLine
        {
            ItemName = x.ItemName,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            BatchNo = x.BatchNo,
            ExpiryDate = x.ExpiryDate,
            Mrp = x.Mrp,
            TaxAmount = x.TaxAmount,
            DiscountAmount = x.DiscountAmount
        }).ToList();

        await purchasesService.SaveAsync(model, lineModels);
        await LoadAsync();
        Status = "Purchase saved";
    }

    private void OnLinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<PurchaseLineEditorRow>())
            {
                item.PropertyChanged += OnLinePropertyChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<PurchaseLineEditorRow>())
            {
                item.PropertyChanged -= OnLinePropertyChanged;
            }
        }

        RecalculateTotal();
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PurchaseLineEditorRow.Quantity)
            or nameof(PurchaseLineEditorRow.UnitPrice)
            or nameof(PurchaseLineEditorRow.LineTotal))
        {
            RecalculateTotal();
        }
    }

    private void RecalculateTotal()
    {
        Total = CalculatedTotal;
        OnPropertyChanged(nameof(CalculatedTotal));
    }
}
