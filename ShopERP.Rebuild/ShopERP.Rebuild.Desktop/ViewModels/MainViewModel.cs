using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShopERP.Rebuild.Core.Contracts;
using ShopERP.Rebuild.Desktop.Services;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;
using System.Windows.Threading;

namespace ShopERP.Rebuild.Desktop.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    public sealed class NavigationItem
    {
        public NavigationItem(string label, string route, bool isEnabled = true)
        {
            Label = label;
            Route = route;
            IsEnabled = isEnabled;
        }

        public string Label { get; }
        public string Route { get; }
        public bool IsEnabled { get; }
    }

    public sealed class NavigationSection
    {
        public NavigationSection(string title, IEnumerable<NavigationItem> items)
        {
            Title = title;
            Items = items.ToList();
        }

        public string Title { get; }
        public IReadOnlyList<NavigationItem> Items { get; }
    }

    private readonly IShellNavigationService _navigationService;
    private readonly ISyncService _syncService;
    private readonly UserSession _userSession;
    private readonly Dictionary<string, PageViewModelBase> _pages;

    [ObservableProperty]
    private PageViewModelBase _currentPage;

    [ObservableProperty]
    private string _syncStatus;

    [ObservableProperty]
    private string _userBadge;

    public IReadOnlyList<NavigationSection> NavigationSections { get; }

    public bool IsAdmin => _userSession.Role == Core.Domain.Enums.UserRole.Admin;

    public MainViewModel(
        DashboardPageViewModel dashboard,
        ProductsPageViewModel products,
        ProductsFormPageViewModel productsForm,
        CustomersPageViewModel customers,
        CustomersFormPageViewModel customersForm,
        DoctorsPageViewModel doctors,
        DoctorsFormPageViewModel doctorsForm,
        SalesPageViewModel sales,
        SalesFormPageViewModel salesForm,
        PurchasesPageViewModel purchases,
        PurchasesFormPageViewModel purchasesForm,
        ReportsPageViewModel reports,
        StockPageViewModel stockPage,
        NotificationsPageViewModel notificationsPage,
        LedgersPageViewModel ledgersPage,
        BillingPageViewModel billingPage,
        BillingFormPageViewModel billingFormPage,
        ReturnsPageViewModel returnsPage,
        ReturnsFormPageViewModel returnsFormPage,
        PaymentsPageViewModel paymentsPage,
        PaymentsFormPageViewModel paymentsFormPage,
        PurchaseOrdersPageViewModel purchaseOrdersPage,
        PurchaseOrdersFormPageViewModel purchaseOrdersFormPage,
        SyncPageViewModel syncPage,
        IShellNavigationService navigationService,
        ISyncService syncService,
        UserSession userSession)
    {
        _navigationService = navigationService;
        _syncService = syncService;
        _userSession = userSession;
        _navigationService.SetNavigateHandler(NavigateCoreAsync);

        _pages = new Dictionary<string, PageViewModelBase>(StringComparer.OrdinalIgnoreCase)
        {
            ["Dashboard"] = dashboard,
            ["ProductsList"] = products,
            ["ProductsForm"] = productsForm,
            ["CustomersList"] = customers,
            ["CustomersForm"] = customersForm,
            ["DoctorsList"] = doctors,
            ["DoctorsForm"] = doctorsForm,
            ["SalesList"] = sales,
            ["SalesForm"] = salesForm,
            ["PurchasesList"] = purchases,
            ["PurchasesForm"] = purchasesForm,
            ["Products"] = products,
            ["Customers"] = customers,
            ["Doctors"] = doctors,
            ["Sales"] = sales,
            ["Purchases"] = purchases,
            ["Reports"] = reports,
            ["Stock"] = stockPage,
            ["Alerts"] = notificationsPage,
            ["Ledgers"] = ledgersPage,
            ["BillingList"] = billingPage,
            ["BillingForm"] = billingFormPage,
            ["Billing"] = billingPage,
            ["ReturnsList"] = returnsPage,
            ["ReturnsForm"] = returnsFormPage,
            ["Returns"] = returnsPage,
            ["PaymentsList"] = paymentsPage,
            ["PaymentsForm"] = paymentsFormPage,
            ["Payments"] = paymentsPage,
            ["OrdersList"] = purchaseOrdersPage,
            ["OrdersForm"] = purchaseOrdersFormPage,
            ["Orders"] = purchaseOrdersPage,
            ["Sync"] = syncPage
        };

        _currentPage = dashboard;
        _syncStatus = "Sync: idle";
        _userBadge = $"{_userSession.Username} ({_userSession.Role})";
        NavigationSections = BuildNavigationSections();

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        timer.Tick += (_, _) =>
        {
            SyncStatus = $"Sync: {_syncService.CurrentHealth} | {_syncService.LastMessage}";
        };
        timer.Start();
    }

    [RelayCommand]
    private async Task Navigate(string route)
    {
        await NavigateCoreAsync(route);
    }

    private async Task NavigateCoreAsync(string route)
    {
        if (!CanAccess(route))
        {
            SyncStatus = "Access denied for selected module.";
            return;
        }

        if (_pages.TryGetValue(route, out var page))
        {
            if (page is IPageActivationAware activationAware)
            {
                await activationAware.OnNavigatedToAsync();
            }

            CurrentPage = page;
        }
    }

    private bool CanRunSyncNow() => IsAdmin;

    [RelayCommand(CanExecute = nameof(CanRunSyncNow))]
    private async Task RunSyncNow()
    {
        if (!IsAdmin)
        {
            SyncStatus = "Only admin can trigger manual sync.";
            return;
        }

        SyncStatus = "Sync: running...";
        await _syncService.SyncAsync();
        SyncStatus = $"Sync: {_syncService.CurrentHealth} | {_syncService.LastMessage}";
    }

    private bool CanAccess(string route)
    {
        if (IsAdmin)
        {
            return true;
        }

        return !string.Equals(route, "Reports", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(route, "Sync", StringComparison.OrdinalIgnoreCase);
    }

    private IReadOnlyList<NavigationSection> BuildNavigationSections()
    {
        return
        [
            new NavigationSection("CORE MODULES",
            [
                new NavigationItem("Clinic Dashboard", "Dashboard")
            ]),
            new NavigationSection("RECORDS & LOGS",
            [
                new NavigationItem("Pharmacy Records", "ProductsList"),
                new NavigationItem("Patient Database", "CustomersList"),
                new NavigationItem("Medical Practitioners", "DoctorsList"),
                new NavigationItem("Billing Registry", "BillingList"),
                new NavigationItem("Medication Orders", "OrdersList"),
                new NavigationItem("Sales Records", "SalesList"),
                new NavigationItem("Procurement", "PurchasesList"),
                new NavigationItem("Return Logs", "ReturnsList"),
                new NavigationItem("Financial Registry", "PaymentsList")
            ]),
            new NavigationSection("DATA ENTRY",
            [
                new NavigationItem("Add Medication", "ProductsForm"),
                new NavigationItem("Register Patient", "CustomersForm"),
                new NavigationItem("Add Practitioner", "DoctorsForm"),
                new NavigationItem("New Billing", "BillingForm"),
                new NavigationItem("Create Order", "OrdersForm")
            ]),
            new NavigationSection("SYSTEM INSIGHTS",
            [
                new NavigationItem("Clinical Reports", "Reports", IsAdmin),
                new NavigationItem("Inventory Levels", "Stock"),
                new NavigationItem("Health Alerts", "Alerts"),
                new NavigationItem("Financial Ledger", "Ledgers"),
                new NavigationItem("Secure Data Sync", "Sync", IsAdmin)
            ])
        ];
    }
}
