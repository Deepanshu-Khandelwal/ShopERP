using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Services;
using ShopERP.Rebuild.Desktop.Models;
using ShopERP.Rebuild.Desktop.Services;
using System.Collections.ObjectModel;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public partial class PurchaseOrdersPageViewModel(
    BackendDbContext dbContext,
    IPurchaseOrderService purchaseOrderService,
    PurchaseOrderDraftStore draftStore,
    IShellNavigationService navigationService) : PageViewModelBase("Purchase Orders")
{
    [ObservableProperty] private ObservableCollection<Supplier> _suppliers = new();
    [ObservableProperty] private ObservableCollection<Product> _products = new();
    [ObservableProperty] private ObservableCollection<PurchaseOrder> _orders = new();
    [ObservableProperty] private ObservableCollection<OrderLineRow> _lines = new();
    [ObservableProperty] private OrderLineRow? _selectedLine;
    [ObservableProperty] private PurchaseOrder? _selectedOrder;
    [ObservableProperty] private int _selectedSupplierId;
    [ObservableProperty] private int _selectedProductId;
    [ObservableProperty] private int _lineQty = 1;
    [ObservableProperty] private decimal _lineRate;
    [ObservableProperty] private string _orderNo = string.Empty;
    [ObservableProperty] private DateTime _orderDate = DateTime.Today;
    [ObservableProperty] private string _convertBillNo = string.Empty;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private string _status = "Ready";

    protected PurchaseOrderDraftStore DraftStore { get; } = draftStore;
    protected IShellNavigationService NavigationService { get; } = navigationService;

    public decimal Total => Lines.Sum(x => x.Amount);
    public string SaveActionLabel => IsEditMode ? "Update Order" : "Create Order";

    partial void OnIsEditModeChanged(bool value)
    {
        OnPropertyChanged(nameof(SaveActionLabel));
    }

    public async Task LoadAsync()
    {
        Suppliers = new ObservableCollection<Supplier>(await dbContext.Suppliers.OrderBy(x => x.Name).ToListAsync(default));
        Products = new ObservableCollection<Product>(await dbContext.Products.OrderBy(x => x.Name).ToListAsync(default));
        Orders = new ObservableCollection<PurchaseOrder>(await dbContext.PurchaseOrders.Include(x => x.Supplier).OrderByDescending(x => x.OrderDate).Take(200).ToListAsync(default));

        if (string.IsNullOrWhiteSpace(OrderNo))
        {
            OrderNo = $"PO-{DateTime.Now:yyyyMMdd-HHmmss}";
        }

        if (string.IsNullOrWhiteSpace(ConvertBillNo))
        {
            ConvertBillNo = $"PB-{DateTime.Now:yyyyMMdd-HHmmss}";
        }

        Status = "Purchase orders loaded";
    }

    [RelayCommand]
    private void AddLine()
    {
        var product = Products.FirstOrDefault(x => x.Id == SelectedProductId);
        if (product is null)
        {
            Status = "Select product.";
            return;
        }

        Lines.Add(new OrderLineRow
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = Math.Max(1, LineQty),
            Rate = LineRate > 0 ? LineRate : product.PurchaseRate
        });

        OnPropertyChanged(nameof(Total));
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
    private async Task CreateOrderAsync()
    {
        if (SelectedSupplierId <= 0 || Lines.Count == 0)
        {
            Status = "Select supplier and add lines.";
            return;
        }

        if (DraftStore.Current is { } draft)
        {
            var existingOrder = await dbContext.PurchaseOrders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == draft.PurchaseOrderId, default);

            if (existingOrder is null)
            {
                Status = "Order not found for update.";
                DraftStore.Clear();
                IsEditMode = false;
                return;
            }

            existingOrder.SupplierId = SelectedSupplierId;
            existingOrder.OrderNo = OrderNo;
            existingOrder.OrderDate = OrderDate;
            existingOrder.TotalAmount = Lines.Sum(x => x.Amount);
            existingOrder.UpdatedAtUtc = DateTime.UtcNow;

            dbContext.PurchaseOrderItems.RemoveRange(existingOrder.Items);
            existingOrder.Items = Lines.Select(x => new PurchaseOrderItem
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Rate = x.Rate,
                Amount = x.Amount
            }).ToList();

            await dbContext.SaveChangesAsync(default);
            await LoadAsync();
            DraftStore.Clear();
            IsEditMode = false;
            ResetEditorFields();
            Status = "Order updated";
            return;
        }

        var request = new PurchaseOrderCreateRequest
        {
            OrderNo = OrderNo,
            SupplierId = SelectedSupplierId,
            OrderDate = OrderDate,
            Items = Lines.Select(x => new PurchaseOrderLineDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Rate = x.Rate
            }).ToList()
        };

        await purchaseOrderService.CreateAsync(request, default);
        Status = "Order created";
        Lines.Clear();
        OrderNo = $"PO-{DateTime.Now:yyyyMMdd-HHmmss}";
        DraftStore.Clear();
        IsEditMode = false;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task MarkSentAsync()
    {
        if (SelectedOrder is null)
        {
            return;
        }

        await purchaseOrderService.MarkSentAsync(SelectedOrder.Id, default);
        Status = "Order marked as sent";
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ConvertToBillAsync()
    {
        if (SelectedOrder is null)
        {
            Status = "Select order to convert.";
            return;
        }

        var billNo = string.IsNullOrWhiteSpace(ConvertBillNo) ? $"PB-{DateTime.Now:yyyyMMdd-HHmmss}" : ConvertBillNo.Trim();
        var bill = await purchaseOrderService.ConvertToPurchaseBillAsync(SelectedOrder.Id, billNo, DateTime.Today, default);
        Status = $"Converted to purchase bill {bill.BillNo}";
        await LoadAsync();
    }

    protected void ResetEditorFields()
    {
        IsEditMode = false;
        SelectedOrder = null;
        SelectedSupplierId = 0;
        SelectedProductId = 0;
        LineQty = 1;
        LineRate = 0;
        OrderNo = string.Empty;
        OrderDate = DateTime.Today;
        ConvertBillNo = string.Empty;
        SelectedLine = null;
        Lines = new ObservableCollection<OrderLineRow>();
    }

    protected void ApplyDraft(PurchaseOrderDraft draft)
    {
        IsEditMode = true;
        SelectedOrder = null;
        SelectedSupplierId = draft.SupplierId;
        OrderNo = draft.OrderNo;
        OrderDate = draft.OrderDate;
        Lines = new ObservableCollection<OrderLineRow>(draft.Lines.Select(line => new OrderLineRow
        {
            ProductId = line.ProductId,
            ProductName = line.ProductName,
            Quantity = line.Quantity,
            Rate = line.Rate
        }));
    }

    [RelayCommand]
    private async Task EditAsync(PurchaseOrder? item)
    {
        if (item is null) return;

        DraftStore.SetFrom(item);
        IsEditMode = true;
        Status = "Opening purchase order draft";
        await NavigationService.NavigateAsync("OrdersForm");
    }

    [RelayCommand]
    private async Task DeleteAsync(PurchaseOrder? item)
    {
        if (item is null) return;

        var order = await dbContext.PurchaseOrders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == item.Id, default);

        if (order is null)
        {
            Status = "Purchase order not found";
            return;
        }

        dbContext.PurchaseOrderItems.RemoveRange(order.Items);
        dbContext.PurchaseOrders.Remove(order);
        await dbContext.SaveChangesAsync(default);
        await LoadAsync();
        Status = "Purchase order deleted";
    }
}
