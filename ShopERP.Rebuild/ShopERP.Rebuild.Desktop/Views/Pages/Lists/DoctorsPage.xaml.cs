using System.Windows.Controls;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;

namespace ShopERP.Rebuild.Desktop.Views.Pages.Lists;

public partial class DoctorsPage : UserControl
{
    public DoctorsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DoctorsPageViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}