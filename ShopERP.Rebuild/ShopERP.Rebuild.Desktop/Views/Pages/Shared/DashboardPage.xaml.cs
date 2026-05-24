using System.Windows.Controls;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;

namespace ShopERP.Rebuild.Desktop.Views.Pages.Shared;

public partial class DashboardPage : UserControl
{
    public DashboardPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DashboardPageViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}
