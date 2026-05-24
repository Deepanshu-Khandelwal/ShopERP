using System.Windows.Controls;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;

namespace ShopERP.Rebuild.Desktop.Views.Pages.Forms;

public partial class DoctorsForm : UserControl
{
    public DoctorsForm()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DoctorsFormPageViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}