namespace ShopERP.Rebuild.Desktop.Services;

public interface IShellNavigationService
{
    void SetNavigateHandler(Func<string, Task> navigateHandler);
    Task NavigateAsync(string route);
}

public sealed class ShellNavigationService : IShellNavigationService
{
    private Func<string, Task>? _navigateHandler;

    public void SetNavigateHandler(Func<string, Task> navigateHandler)
    {
        _navigateHandler = navigateHandler;
    }

    public Task NavigateAsync(string route)
    {
        return _navigateHandler?.Invoke(route) ?? Task.CompletedTask;
    }
}