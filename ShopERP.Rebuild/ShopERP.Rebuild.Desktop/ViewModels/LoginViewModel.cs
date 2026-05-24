using CommunityToolkit.Mvvm.ComponentModel;
using ShopERP.Rebuild.Core.Contracts;
using ShopERP.Rebuild.Desktop.Services;

namespace ShopERP.Rebuild.Desktop.ViewModels;

public sealed partial class LoginViewModel(
    IAuthenticationService authenticationService,
    UserSession userSession) : ViewModelBase
{
    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _error = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public async Task<bool> LoginAsync(string password)
    {
        Error = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            Error = "Enter username and password.";
            return false;
        }

        IsBusy = true;
        try
        {
            var user = await authenticationService.LoginAsync(Username, password);
            if (user is null)
            {
                Error = "Invalid credentials.";
                return false;
            }

            userSession.Start(user.Username, user.Role);
            return true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
