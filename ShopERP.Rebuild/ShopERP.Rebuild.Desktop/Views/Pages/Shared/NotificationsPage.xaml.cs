using System.Windows.Controls;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;

namespace ShopERP.Rebuild.Desktop.Views.Pages.Shared;

public partial class NotificationsPage : UserControl
{
    public NotificationsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is NotificationsPageViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}