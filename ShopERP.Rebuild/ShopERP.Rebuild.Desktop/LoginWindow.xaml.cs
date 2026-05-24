using ShopERP.Rebuild.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace ShopERP.Rebuild.Desktop;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private readonly IServiceProvider _services;

    public LoginWindow(LoginViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _services = services;
        DataContext = _viewModel;
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var ok = await _viewModel.LoginAsync(PasswordInput.Password);

            if (ok)
            {
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Login failed: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnRegisterClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var registrationVm = _services.GetRequiredService<RegistrationViewModel>();
            var registrationWindow = new RegistrationWindow(registrationVm, _services)
            {
                Owner = this
            };

            Hide();
            registrationWindow.ShowDialog();
            Show();
            Activate();
        }
        catch (Exception ex)
        {
            Show();
            Activate();
            MessageBox.Show($"Failed to open registration window: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}