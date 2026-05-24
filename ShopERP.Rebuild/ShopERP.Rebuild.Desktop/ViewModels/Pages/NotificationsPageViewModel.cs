using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Services;
using System.Collections.ObjectModel;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed partial class NotificationsPageViewModel(
    BackendDbContext dbContext,
    INotificationService notificationService) : PageViewModelBase("Alerts")
{
    [ObservableProperty]
    private ObservableCollection<NotificationEntry> _items = new();

    [ObservableProperty]
    private string _status = "Ready";

    public async Task LoadAsync()
    {
        var rows = await dbContext.NotificationEntries
            .OrderByDescending(x => x.TriggeredAtUtc)
            .Take(200)
            .ToListAsync(default);

        Items = new ObservableCollection<NotificationEntry>(rows);
        Status = $"Loaded {Items.Count} notifications";
    }

    [RelayCommand]
    private async Task GenerateAsync()
    {
        await notificationService.GenerateSystemNotificationsAsync(default);
        await LoadAsync();
        Status = "Notifications refreshed";
    }
}
