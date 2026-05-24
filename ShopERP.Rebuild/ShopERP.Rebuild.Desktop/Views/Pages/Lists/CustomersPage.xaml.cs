using System.Windows.Controls;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;

namespace ShopERP.Rebuild.Desktop.Views.Pages.Lists;

public partial class CustomersPage : UserControl
{
    public CustomersPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is CustomersPageViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}
