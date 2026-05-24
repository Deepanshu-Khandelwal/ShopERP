using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;
using ShopERP.Backend.Domain.Entities;
using System.Collections.ObjectModel;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed partial class StockPageViewModel(BackendDbContext dbContext) : PageViewModelBase("Stock Batches")
{
    [ObservableProperty]
    private ObservableCollection<StockBatch> _items = new();

    [ObservableProperty]
    private string _status = "Ready";

    public async Task LoadAsync()
    {
        var rows = await dbContext.StockBatches
            .Include(x => x.Product)
            .OrderBy(x => x.Product.Name)
            .ThenBy(x => x.ExpiryDate)
            .ToListAsync(default);

        Items = new ObservableCollection<StockBatch>(rows);
        Status = $"Loaded {Items.Count} stock batches";
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();

    [RelayCommand]
    private void Edit(StockBatch? item)
    {
        if (item is null) return;
        Status = $"Stock batch {item.BatchNo} selected";
    }

    [RelayCommand]
    private async Task DeleteAsync(StockBatch? item)
    {
        if (item is null) return;

        var movements = await dbContext.StockMovements.Where(x => x.StockBatchId == item.Id).ToListAsync(default);
        dbContext.StockMovements.RemoveRange(movements);
        dbContext.StockBatches.Remove(item);
        await dbContext.SaveChangesAsync(default);
        await LoadAsync();
        Status = "Stock batch deleted";
    }
}
