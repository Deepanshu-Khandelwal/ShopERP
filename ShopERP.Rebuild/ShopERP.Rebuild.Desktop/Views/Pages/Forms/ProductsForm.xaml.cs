using System.Windows.Controls;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;

namespace ShopERP.Rebuild.Desktop.Views.Pages.Forms;

public partial class ProductsForm : UserControl
{
    public ProductsForm()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ProductsFormPageViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}
