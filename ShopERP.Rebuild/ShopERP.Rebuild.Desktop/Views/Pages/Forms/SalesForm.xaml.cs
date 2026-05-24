using System.Windows.Controls;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;

namespace ShopERP.Rebuild.Desktop.Views.Pages.Forms;

public partial class SalesForm : UserControl
{
    public SalesForm()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is SalesFormPageViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}
