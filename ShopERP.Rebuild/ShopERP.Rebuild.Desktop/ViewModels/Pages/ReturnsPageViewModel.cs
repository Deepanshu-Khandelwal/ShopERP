using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Services;
using ShopERP.Rebuild.Desktop.Models;
using System.Collections.ObjectModel;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public enum ReturnMode
{
    SalesReturn,
    PurchaseReturn
}

public partial class ReturnsPageViewModel(
    BackendDbContext dbContext,
    IReturnService returnService) : PageViewModelBase("Returns")
{
    [ObservableProperty] private ReturnMode _selectedMode = ReturnMode.SalesReturn;
    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private ObservableCollection<Supplier> _suppliers = new();
    [ObservableProperty] private ObservableCollection<StockBatch> _batches = new();
    [ObservableProperty] private ObservableCollection<ReturnLineRow> _lines = new();
    [ObservableProperty] private ReturnLineRow? _selectedLine;
    [ObservableProperty] private int? _selectedCustomerId;
    [ObservableProperty] private int? _selectedSupplierId;
    [ObservableProperty] private int _selectedBatchId;
    [ObservableProperty] private int _lineQty = 1;
    [ObservableProperty] private decimal _lineRate;
    [ObservableProperty] private DateTime _returnDate = DateTime.Today;
    [ObservableProperty] private string _returnNo = string.Empty;
    [ObservableProperty] private string _status = "Ready";

    public Array ReturnModes => Enum.GetValues(typeof(ReturnMode));
    public decimal Total => Lines.Sum(x => x.Amount);

    public async Task LoadAsync()
    {
        Customers = new ObservableCollection<Customer>(await dbContext.Customers.OrderBy(x => x.Name).ToListAsync(default));
        Suppliers = new ObservableCollection<Supplier>(await dbContext.Suppliers.OrderBy(x => x.Name).ToListAsync(default));
        Batches = new ObservableCollection<StockBatch>(await dbContext.StockBatches.Include(x => x.Product).OrderBy(x => x.ExpiryDate).ToListAsync(default));
        if (string.IsNullOrWhiteSpace(ReturnNo))
        {
            ReturnNo = $"RT-{DateTime.Now:yyyyMMdd-HHmmss}";
        }
        Status = "Returns data loaded";
    }

    [RelayCommand]
    private void AddLine()
    {
        var batch = Batches.FirstOrDefault(x => x.Id == SelectedBatchId);
        if (batch is null)
        {
            Status = "Select batch.";
            return;
        }

        Lines.Add(new ReturnLineRow
        {
            ProductId = batch.ProductId,
            ProductName = batch.Product.Name,
            StockBatchId = batch.Id,
            BatchNo = batch.BatchNo,
            Quantity = Math.Max(1, LineQty),
            Rate = LineRate > 0 ? LineRate : batch.SaleRate
        });

        OnPropertyChanged(nameof(Total));
        Status = "Return line added";
    }

    [RelayCommand]
    private void RemoveLine()
    {
        if (SelectedLine is null)
        {
            return;
        }

        Lines.Remove(SelectedLine);
        OnPropertyChanged(nameof(Total));
    }

    [RelayCommand]
    private async Task SaveReturnAsync()
    {
        if (Lines.Count == 0)
        {
            Status = "Add at least one return line.";
            return;
        }

        var request = new ReturnCreateRequest
        {
            ReturnNo = ReturnNo,
            ReturnDate = ReturnDate,
            CustomerId = SelectedMode == ReturnMode.SalesReturn ? SelectedCustomerId : null,
            SupplierId = SelectedMode == ReturnMode.PurchaseReturn ? SelectedSupplierId : null,
            Items = Lines.Select(x => new ReturnLineDto
            {
                ProductId = x.ProductId,
                StockBatchId = x.StockBatchId,
                Quantity = x.Quantity,
                Rate = x.Rate
            }).ToList()
        };

        if (SelectedMode == ReturnMode.SalesReturn)
        {
            await returnService.CreateSalesReturnAsync(request, default);
        }
        else
        {
            await returnService.CreatePurchaseReturnAsync(request, default);
        }

        Status = "Return saved";
        Lines.Clear();
        ReturnNo = $"RT-{DateTime.Now:yyyyMMdd-HHmmss}";
        await LoadAsync();
    }
}
