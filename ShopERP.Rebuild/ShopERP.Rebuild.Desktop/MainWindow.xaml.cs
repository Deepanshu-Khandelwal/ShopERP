using System;
using System.Windows;
using ShopERP.Rebuild.Desktop.ViewModels;

namespace ShopERP.Rebuild.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private async void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Application.Current is App app)
                    await app.LogoutAsync(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to log out: {ex.Message}\n\n{ex.InnerException?.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}