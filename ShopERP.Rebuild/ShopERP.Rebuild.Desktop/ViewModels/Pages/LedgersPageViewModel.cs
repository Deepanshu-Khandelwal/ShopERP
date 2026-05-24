using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;
using System.Collections.ObjectModel;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed partial class LedgersPageViewModel(BackendDbContext dbContext) : PageViewModelBase("Ledgers")
{
    [ObservableProperty]
    private ObservableCollection<CustomerLedgerRow> _customerLedgers = new();

    [ObservableProperty]
    private ObservableCollection<SupplierLedgerRow> _supplierLedgers = new();

    [ObservableProperty]
    private string _status = "Ready";

    [RelayCommand]
    private void Edit(object? item)
    {
        if (item is null) return;
        if (item is CustomerLedgerRow customerRow)
        {
            Status = $"Customer ledger summary selected: {customerRow.CustomerName}";
            return;
        }

        if (item is SupplierLedgerRow supplierRow)
        {
            Status = $"Supplier ledger summary selected: {supplierRow.SupplierName}";
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(object? item)
    {
        if (item is null) return;

        if (item is CustomerLedgerRow customerRow)
        {
            var entries = await dbContext.CustomerLedgerEntries.Where(x => x.CustomerId == customerRow.CustomerId).ToListAsync(default);
            dbContext.CustomerLedgerEntries.RemoveRange(entries);
            await dbContext.SaveChangesAsync(default);
            await LoadAsync();
            Status = $"Deleted customer ledger entries for {customerRow.CustomerName}";
            return;
        }

        if (item is SupplierLedgerRow supplierRow)
        {
            var entries = await dbContext.SupplierLedgerEntries.Where(x => x.SupplierId == supplierRow.SupplierId).ToListAsync(default);
            dbContext.SupplierLedgerEntries.RemoveRange(entries);
            await dbContext.SaveChangesAsync(default);
            await LoadAsync();
            Status = $"Deleted supplier ledger entries for {supplierRow.SupplierName}";
            return;
        }

        Status = "Unsupported ledger row";
    }

    public async Task LoadAsync()
    {
        var customers = (await dbContext.CustomerLedgerEntries
            .Include(x => x.Customer)
            .ToListAsync(default))
            .GroupBy(x => new { CustomerId = x.CustomerId, CustomerName = x.Customer?.Name ?? "Unknown Customer" })
            .Select(g => new CustomerLedgerRow(
                g.Key.CustomerId,
                g.Key.CustomerName,
                g.OrderByDescending(x => x.Id).Select(x => x.Balance).FirstOrDefault(),
                g.Count()))
            .OrderByDescending(x => x.Balance)
            .ToList();

        var suppliers = (await dbContext.SupplierLedgerEntries
            .Include(x => x.Supplier)
            .ToListAsync(default))
            .GroupBy(x => new { SupplierId = x.SupplierId, SupplierName = x.Supplier?.Name ?? "Unknown Supplier" })
            .Select(g => new SupplierLedgerRow(
                g.Key.SupplierId,
                g.Key.SupplierName,
                g.OrderByDescending(x => x.Id).Select(x => x.Balance).FirstOrDefault(),
                g.Count()))
            .OrderByDescending(x => x.Balance)
            .ToList();

        CustomerLedgers = new ObservableCollection<CustomerLedgerRow>(customers);
        SupplierLedgers = new ObservableCollection<SupplierLedgerRow>(suppliers);
        Status = $"Loaded {customers.Count} customer and {suppliers.Count} supplier ledgers";
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();
}

public sealed record CustomerLedgerRow(int CustomerId, string CustomerName, decimal Balance, int Entries);
public sealed record SupplierLedgerRow(int SupplierId, string SupplierName, decimal Balance, int Entries);
