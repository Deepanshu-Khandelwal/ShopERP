using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Responses;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Services;
using System.Collections.ObjectModel;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed partial class ReportsPageViewModel(
    IReportService reportService,
    BackendDbContext dbContext) : PageViewModelBase("Reports")
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
    private ObservableCollection<TopProductDto> _topProducts = new();

    [ObservableProperty]
    private ObservableCollection<DoctorSalesRow> _doctorWiseSales = new();

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private int _selectedCustomerId;

    [ObservableProperty]
    private ObservableCollection<CustomerPurchaseHistoryRow> _customerPurchaseHistory = new();

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

        var doctors = (await dbContext.SalesBills
            .Where(x => x.DoctorId.HasValue)
            .Include(x => x.Doctor)
            .ToListAsync(default))
            .GroupBy(x => new { DoctorId = x.DoctorId ?? 0, DoctorName = x.Doctor?.Name ?? "Unknown Doctor" })
            .Select(g => new DoctorSalesRow(g.Key.DoctorId, g.Key.DoctorName, g.Count(), g.Sum(x => x.GrandTotal)))
            .OrderByDescending(x => x.TotalAmount)
            .Take(20)
            .ToList();
        DoctorWiseSales = new ObservableCollection<DoctorSalesRow>(doctors);

        var customerRows = await dbContext.Customers.OrderBy(x => x.Name).ToListAsync(default);
        Customers = new ObservableCollection<Customer>(customerRows);

        if (SelectedCustomerId == 0 && customerRows.Count > 0)
        {
            SelectedCustomerId = customerRows[0].Id;
        }

        await LoadCustomerHistoryAsync();
        Status = "Loaded report summary";
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();

    [RelayCommand]
    private Task LoadCustomerHistoryAsync()
    {
        return LoadCustomerHistoryCoreAsync();
    }

    private async Task LoadCustomerHistoryCoreAsync()
    {
        if (SelectedCustomerId <= 0)
        {
            CustomerPurchaseHistory = new ObservableCollection<CustomerPurchaseHistoryRow>();
            return;
        }

        var history = await dbContext.SalesBills
            .Where(x => x.CustomerId == SelectedCustomerId)
            .OrderByDescending(x => x.BillDate)
            .Select(x => new CustomerPurchaseHistoryRow(x.Id, x.BillNo, x.BillDate, x.GrandTotal, x.PaidAmount, x.DueAmount))
            .Take(100)
            .ToListAsync(default);

        CustomerPurchaseHistory = new ObservableCollection<CustomerPurchaseHistoryRow>(history);
    }
}

public sealed record DoctorSalesRow(int DoctorId, string DoctorName, int Bills, decimal TotalAmount);
public sealed record CustomerPurchaseHistoryRow(int BillId, string BillNo, DateTime BillDate, decimal GrandTotal, decimal PaidAmount, decimal DueAmount);
