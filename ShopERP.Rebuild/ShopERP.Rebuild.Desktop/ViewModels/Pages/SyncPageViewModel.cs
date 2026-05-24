namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed class SyncPageViewModel : PageViewModelBase
{
    public SyncPageViewModel() : base("Secure Sync")
    {
    }

    public string Description => "Local-first SQLite with encrypted background replication to MySQL backup.";
}
