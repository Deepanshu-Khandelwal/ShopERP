namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public abstract class PageViewModelBase : ViewModelBase
{
    protected PageViewModelBase(string title)
    {
        Title = title;
    }

    public string Title { get; }
}
