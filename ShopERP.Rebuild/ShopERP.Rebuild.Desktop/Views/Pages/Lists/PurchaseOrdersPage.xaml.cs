using System.Windows.Controls;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;

namespace ShopERP.Rebuild.Desktop.Views.Pages.Lists;

public partial class PurchaseOrdersPage : UserControl
{
    public PurchaseOrdersPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is PurchaseOrdersPageViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}
