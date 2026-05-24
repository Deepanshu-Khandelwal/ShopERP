using System.Windows.Controls;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;

namespace ShopERP.Rebuild.Desktop.Views.Pages.Lists;

public partial class LedgersPage : UserControl
{
    public LedgersPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LedgersPageViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}