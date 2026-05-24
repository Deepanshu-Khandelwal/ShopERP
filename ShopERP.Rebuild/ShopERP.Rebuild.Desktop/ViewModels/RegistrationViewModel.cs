using CommunityToolkit.Mvvm.ComponentModel;
using ShopERP.Rebuild.Core.Contracts;

namespace ShopERP.Rebuild.Desktop.ViewModels;

public sealed partial class RegistrationViewModel(
    IAuthenticationService authenticationService) : ViewModelBase
{
    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _error = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public async Task<bool> RegisterAsync()
    {
        Error = string.Empty;
        SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            Error = "Username is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            Error = "Password is required.";
            return false;
        }

        IsBusy = true;
        try
        {
            var (success, errorMsg) = await authenticationService.RegisterAsync(
                Username.Trim(),
                Password,
                ConfirmPassword);

            if (!success)
            {
                Error = errorMsg ?? "Registration failed.";
                return false;
            }

            SuccessMessage = "Account created successfully! You can now log in.";
            ClearFields();
            return true;
        }
        catch (Exception ex)
        {
            Error = $"An error occurred: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearFields()
    {
        Username = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }
}
