using ShopERP.Rebuild.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ShopERP.Rebuild.Desktop;

public partial class RegistrationWindow : Window
{
    private readonly RegistrationViewModel _viewModel;
    private readonly IServiceProvider _services;

    public RegistrationWindow(RegistrationViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _services = services;
        DataContext = _viewModel;
    }

    private async void OnRegisterClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _viewModel.Password = PasswordInput.Password;
            _viewModel.ConfirmPassword = ConfirmPasswordInput.Password;

            var ok = await _viewModel.RegisterAsync();
            if (ok)
            {
                PasswordInput.Clear();
                ConfirmPasswordInput.Clear();

                // Keep success feedback visible briefly, then return to the existing login dialog.
                await Task.Delay(2000);
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Registration error: {ex.Message}\n\n{ex.InnerException?.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnSignInClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
