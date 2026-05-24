using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Responses;
using ShopERP.Backend.Services;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;
using System.Collections.ObjectModel;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed partial class PharmacyDashboardViewModel(
    IReportService reportService,
    BackendDbContext dbContext,
    INotificationService notificationService) : PageViewModelBase("Dashboard")
{
    [ObservableProperty]
    private decimal _totalSales;

    [ObservableProperty]
    private decimal _totalPurchases;

    [ObservableProperty]
    private decimal _totalProfit;

    [ObservableProperty]
    private decimal _totalGst;

    [ObservableProperty]
    private int _lowStockAlerts;

    [ObservableProperty]
    private int _expiryAlerts;

    [ObservableProperty]
    private ObservableCollection<TopProductDto> _topProducts = new();

    [ObservableProperty]
    private string _status = "Ready";

    public async Task LoadAsync()
    {
        var today = DateTime.UtcNow.Date;
        var summary = await reportService.GetDashboardSummaryAsync(today.AddDays(-30), today.AddDays(1), default);

        TotalSales = summary.TotalSales;
        TotalPurchases = summary.TotalPurchases;
        TotalProfit = summary.TotalProfit;
        TotalGst = summary.TotalGst;
        TopProducts = new ObservableCollection<TopProductDto>(summary.TopProducts);

        LowStockAlerts = await dbContext.StockBatches
            .Include(x => x.Product)
            .CountAsync(x => x.Quantity <= x.Product.LowStockThreshold, default);

        var expiryDate = today.AddDays(90);
        ExpiryAlerts = await dbContext.StockBatches
            .CountAsync(x => x.Quantity > 0 && x.ExpiryDate >= today && x.ExpiryDate <= expiryDate, default);

        Status = $"Loaded {TopProducts.Count} top products";
    }

    [RelayCommand]
    private async Task GenerateNotificationsAsync()
    {
        await notificationService.GenerateSystemNotificationsAsync(default);
        Status = "System notifications generated";
        await LoadAsync();
    }
}
